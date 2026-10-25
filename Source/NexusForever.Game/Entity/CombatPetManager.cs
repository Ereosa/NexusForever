using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Info;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Spell;
using NexusForever.Game.Static.Pet;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Entity;
using NexusForever.Network.World.Message.Model;
using NexusForever.Network.World.Message.Model.Abilities;
using NexusForever.Network.World.Message.Model.Pet;
using NLog;

namespace NexusForever.Game.Entity
{
    public class CombatPetManager : ICombatPetManager
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Engineer bot summon tooltips: "Two bots can be active at one time.", summoning another dismisses the oldest.
        /// </summary>
        private const int MaxBots = 2;

        /// <summary>
        /// Engineer pet bar: Attack, Stop and Follow commands and the 41693 Go To Location spell.
        /// </summary>
        private const ushort BotActionBarShortcutSetId = 299;

        /// <summary>
        /// 56487 Engineer - Limiter, a never ending buff proxied by every bot summon.
        /// </summary>
        /// <remarks>
        /// Spell stack groups aren't implemented, only the latest limiter is kept so they don't build up on the player.
        /// </remarks>
        private const uint LimiterSpell4BaseId = 36129;

        private const uint ShortcutTypeSpell       = 6;
        private const uint ShortcutTypeSpell2      = 7;
        private const uint ShortcutTypeUnitCommand = 11;
        private const uint ShortcutTypeCommand     = 12;

        private sealed class CombatPetSummon
        {
            public ISpell Spell { get; init; }
            public ISpellTargetEffectInfo Info { get; init; }
            public ISpellEffectSummonPetData Data { get; init; }
            public ICreatureInfo CreatureInfo { get; init; }
            public bool IsBot { get; init; }
            public int FollowSlot { get; init; }
            public uint? Guid { get; set; }
        }

        private readonly IPlayer owner;
        private readonly List<CombatPetSummon> summons = [];

        /// <summary>
        /// Stance for all engineer pets, new bots are summoned with it.
        /// </summary>
        /// <remarks>
        /// Defensive is assumed to be the retail default, the client data doesn't include it.
        /// </remarks>
        private PetStance stance = PetStance.Defensive;

        private ushort shownActionBarShortcutSetId;
        private uint lastAbilityCastingId;

        #region Dependency Injection

        private readonly IEntityFactory entityFactory;
        private readonly ICreatureInfoManager creatureInfoManager;
        private readonly ISpellInfoManager spellInfoManager;
        private readonly IGameTableManager gameTableManager;

        public CombatPetManager(IPlayer owner,
            IEntityFactory entityFactory,
            ICreatureInfoManager creatureInfoManager,
            ISpellInfoManager spellInfoManager,
            IGameTableManager gameTableManager)
        {
            this.owner               = owner;
            this.entityFactory       = entityFactory;
            this.creatureInfoManager = creatureInfoManager;
            this.spellInfoManager    = spellInfoManager;
            this.gameTableManager    = gameTableManager;
        }

        #endregion

        /// <summary>
        /// Returns all currently spawned <see cref="ICombatPetEntity"/>.
        /// </summary>
        public IEnumerable<ICombatPetEntity> GetPets()
        {
            if (owner.Map == null)
                yield break;

            foreach (CombatPetSummon summon in summons.ToList())
            {
                if (summon.Guid == null)
                    continue;

                ICombatPetEntity pet = owner.Map.GetEntity<ICombatPetEntity>(summon.Guid.Value);
                if (pet != null)
                    yield return pet;
            }
        }

        /// <summary>
        /// Return the spawned <see cref="ICombatPetEntity"/> summoned by the supplied spell4 id.
        /// </summary>
        public ICombatPetEntity GetPetBySummonSpell(uint spell4Id)
        {
            return GetPets().FirstOrDefault(p => p.SummonSpell4Id == spell4Id);
        }

        /// <summary>
        /// Summon a pet for the SummonPet effect of the supplied <see cref="ISpell"/>.
        /// </summary>
        public bool Summon(ISpell spell, ISpellTargetEffectInfo info, ISpellEffectSummonPetData data)
        {
            if (owner.Map == null)
                return false;

            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(data.CreatureId);
            if (creatureInfo == null)
            {
                log.Warn($"Unable to summon pet, creature {data.CreatureId} for spell {spell.Spell4Id} doesn't exist!");
                return false;
            }

            uint spell4BaseId = spell.Parameters.SpellInfo.BaseInfo.Entry.Id;
            bool isBot = CombatPetAbilityInfo.Get(spell4BaseId) != null;

            // a summon spell only has one pet at a time
            foreach (CombatPetSummon existing in summons
                .Where(s => s.Spell != spell && s.Spell.Parameters.SpellInfo.BaseInfo.Entry.Id == spell4BaseId)
                .ToList())
                Dismiss(existing);

            if (isBot)
            {
                List<CombatPetSummon> bots = summons
                    .Where(s => s.IsBot)
                    .ToList();
                for (int i = 0; i <= bots.Count - MaxBots; i++)
                    Dismiss(bots[i]);

                FinishLimiter();
            }

            var summon = new CombatPetSummon
            {
                Spell        = spell,
                Info         = info,
                Data         = data,
                CreatureInfo = creatureInfo,
                IsBot        = isBot,
                FollowSlot   = GetFreeFollowSlot()
            };
            summons.Add(summon);

            Spawn(summon, owner.Position);
            return true;
        }

        private int GetFreeFollowSlot()
        {
            int slot = 0;
            while (summons.Any(s => s.FollowSlot == slot))
                slot++;
            return slot;
        }

        private void Spawn(CombatPetSummon summon, Vector3 position)
        {
            var pet = entityFactory.CreateEntity<ICombatPetEntity>();
            pet.Initialise(owner, summon.Spell.Parameters.SpellInfo, summon.Data, summon.CreatureInfo, stance, summon.FollowSlot);
            pet.Rotation = owner.Rotation;
            pet.AddToMap(owner.Map, position);
        }

        /// <summary>
        /// Dismiss the pet by ending the summon spell, the SummonPet effect remove handler will then remove the pet.
        /// </summary>
        private void Dismiss(CombatPetSummon summon)
        {
            if (!summon.Spell.IsFinished)
                summon.Spell.Finish();

            // the effect remove handler will normally have done this already
            if (summons.Contains(summon))
                Remove(summon);
        }

        /// <summary>
        /// Remove the pet summoned by the SummonPet effect, invoked when the effect is removed.
        /// </summary>
        public void Unsummon(ISpellTargetEffectInfo info)
        {
            CombatPetSummon summon = summons.FirstOrDefault(s => s.Info == info);
            if (summon != null)
                Remove(summon);
        }

        private void Remove(CombatPetSummon summon)
        {
            summons.Remove(summon);
            if (summon.IsBot && !summons.Any(s => s.IsBot))
                FinishLimiter();

            if (summon.Guid == null)
                return;

            ICombatPetEntity pet = owner.Map?.GetEntity<ICombatPetEntity>(summon.Guid.Value);
            if (pet != null && pet.InWorld)
                pet.RemoveFromMap();

            if (!owner.IsLoading)
                owner.Session.EnqueueMessageEncrypted(new ServerPetDespawned
                {
                    PetUnitId = summon.Guid.Value
                });

            summon.Guid = null;
            UpdateActionBar();
        }

        private void FinishLimiter()
        {
            owner.FinishSpells(s => s.Parameters.SpellInfo.BaseInfo.Entry.Id == LimiterSpell4BaseId);
        }

        /// <summary>
        /// Invoked when <see cref="ICombatPetEntity"/> is added to the map.
        /// </summary>
        public void OnPetAddToMap(ICombatPetEntity pet)
        {
            CombatPetSummon summon = summons.FirstOrDefault(s => s.Guid == null && s.Spell.Spell4Id == pet.SummonSpell4Id);
            if (summon == null)
            {
                // the summon was removed before the pet was added to the map
                pet.RemoveFromMap();
                return;
            }

            summon.Guid = pet.Guid;

            owner.Session.EnqueueMessageEncrypted(new ServerPetSpawned
            {
                PetUnitId         = pet.Guid,
                SummoningSpell4Id = pet.SummonSpell4Id,
                ValidStances      = (uint)pet.ValidStances,
                Stance            = (uint)pet.Stance
            });

            UpdateActionBar();
        }

        /// <summary>
        /// Show the pet bar while engineer pets are summoned.
        /// </summary>
        private void UpdateActionBar()
        {
            ICombatPetEntity bot = GetPets().FirstOrDefault(p => p.ValidStances != PetStance.None);
            ushort setId = bot != null ? BotActionBarShortcutSetId : (ushort)0;
            if (setId == shownActionBarShortcutSetId)
                return;

            shownActionBarShortcutSetId = setId;
            if (owner.IsLoading)
                return;

            owner.Session.EnqueueMessageEncrypted(new ServerShowActionBar
            {
                ShortcutSet            = ShortcutSet.PrimaryPetBar,
                ActionBarShortcutSetId = setId,
                AssociatedUnitId       = bot?.Guid ?? 0u
            });
        }

        /// <summary>
        /// Invoked when <see cref="ICombatPetEntity"/> is killed, the summon effect is cancelled which dismisses the pet.
        /// </summary>
        public void OnPetDeath(ICombatPetEntity pet)
        {
            CombatPetSummon summon = summons.FirstOrDefault(s => s.Guid == pet.Guid);
            if (summon != null)
                Dismiss(summon);
        }

        /// <summary>
        /// Invoked when the owner is removed from the map, pets are removed and summoned again when the owner is added to a map.
        /// </summary>
        public void OnRemoveFromMap()
        {
            foreach (CombatPetSummon summon in summons)
            {
                if (summon.Guid == null)
                    continue;

                ICombatPetEntity pet = owner.Map?.GetEntity<ICombatPetEntity>(summon.Guid.Value);
                if (pet != null && pet.InWorld)
                    pet.RemoveFromMap();

                owner.Session.EnqueueMessageEncrypted(new ServerPetDespawned
                {
                    PetUnitId = summon.Guid.Value
                });

                summon.Guid = null;
            }

            shownActionBarShortcutSetId = 0;
        }

        /// <summary>
        /// Invoked when the owner is added to the map.
        /// </summary>
        public void OnAddToMap()
        {
            foreach (CombatPetSummon summon in summons.ToList())
            {
                if (summon.Spell.IsFinished)
                {
                    summons.Remove(summon);
                    continue;
                }

                if (summon.Guid == null)
                    Spawn(summon, owner.Position);
            }
        }

        /// <summary>
        /// Invoked when the owner is relocated on the same map.
        /// </summary>
        public void OnRelocate(Vector3 position)
        {
            foreach (ICombatPetEntity pet in GetPets())
            {
                pet.CommandFollow();
                pet.RelocateOnMap(position);
            }
        }

        /// <summary>
        /// Invoked when the owner dies, all pets are dismissed.
        /// </summary>
        public void OnDeath()
        {
            foreach (CombatPetSummon summon in summons.ToList())
                Dismiss(summon);
        }

        /// <summary>
        /// Invoked when a hostile unit is added to the owner's threat list.
        /// </summary>
        public void OnOwnerThreatAdd(IUnitEntity hostile)
        {
            foreach (ICombatPetEntity pet in GetPets())
                pet.OnOwnerThreatAdd(hostile);
        }

        /// <summary>
        /// Returns the bot ability spell to cast instead of the supplied summon spell if its pet is already summoned.
        /// </summary>
        /// <remarks>
        /// The client switches the action bar button of a summon spell to its Spell4IdPetSwitch spell while the pet summoned by it exists.
        /// </remarks>
        public ISpellInfo GetPetSwitchSpell(ISpellInfo spellInfo)
        {
            if (spellInfo.Entry.Spell4IdPetSwitch == 0u)
                return null;

            if (!GetPets().Any(p => p.IsAlive && gameTableManager.Spell4.GetEntry(p.SummonSpell4Id)?.Spell4BaseIdBaseSpell == spellInfo.BaseInfo.Entry.Id))
                return null;

            Spell4Entry petSwitchEntry = gameTableManager.Spell4.GetEntry(spellInfo.Entry.Spell4IdPetSwitch);
            if (petSwitchEntry == null)
                return null;

            return spellInfoManager.GetSpellBaseInfo(petSwitchEntry.Spell4BaseIdBaseSpell)?.GetSpellInfo((byte)petSwitchEntry.TierIndex);
        }

        /// <summary>
        /// Invoked when a bot ability (pet switch) spell executes on a target.
        /// </summary>
        public void OnPetAbility(ISpell spell, IUnitEntity target)
        {
            if (lastAbilityCastingId == spell.CastingId)
                return;

            CombatPetAbilityInfo abilityInfo = CombatPetAbilityInfo.GetByPetSwitch(spell.Parameters.SpellInfo.BaseInfo.Entry.Id);
            if (abilityInfo == null || target == null || target == owner)
                return;

            lastAbilityCastingId = spell.CastingId;

            ICombatPetEntity pet = GetPets()
                .FirstOrDefault(p => p.IsAlive && gameTableManager.Spell4.GetEntry(p.SummonSpell4Id)?.Spell4BaseIdBaseSpell == abilityInfo.SummonSpell4BaseId);
            if (pet == null)
                return;

            byte tier = (byte)spell.Parameters.SpellInfo.Entry.TierIndex;
            switch (abilityInfo.AbilityAction)
            {
                case CombatPetAbilityAction.OwnerCast:
                {
                    foreach (uint spell4BaseId in abilityInfo.AbilitySpell4BaseIds)
                    {
                        ISpellInfo spellInfo = spellInfoManager.GetSpellBaseInfo(spell4BaseId)?.GetSpellInfo(tier);
                        if (spellInfo == null)
                            continue;

                        owner.CastSpell(new SpellParameters
                        {
                            SpellInfo              = spellInfo,
                            ParentSpellInfo        = spell.Parameters.SpellInfo,
                            RootSpellInfo          = spell.Parameters.RootSpellInfo,
                            PrimaryTargetId        = target.Guid,
                            UserInitiatedSpellCast = false,
                            IsProxy                = true
                        });
                    }
                    break;
                }
                case CombatPetAbilityAction.PetLeapCast:
                    pet.LeapCast(target, abilityInfo.AbilitySpell4BaseIds);
                    break;
            }
        }

        /// <summary>
        /// Set the stance of the supplied pet, or all pets if guid is 0.
        /// </summary>
        public void SetStance(uint petGuid, PetStance newStance)
        {
            if (petGuid == 0u)
                stance = newStance;

            foreach (ICombatPetEntity pet in GetPets().Where(p => petGuid == 0u || p.Guid == petGuid))
            {
                if (!pet.SetStance(newStance))
                    continue;

                owner.Session.EnqueueMessageEncrypted(new ServerPetStanceChanged
                {
                    PetUnitId = pet.Guid,
                    Stance    = pet.Stance
                });
            }
        }

        private (uint Type, uint ObjectId)? GetShownShortcut(ShortcutSet shortcutSet, uint slot)
        {
            if (shortcutSet != ShortcutSet.PrimaryPetBar || shownActionBarShortcutSetId == 0)
                return null;

            ActionBarShortcutSetEntry entry = gameTableManager.ActionBarShortcutSet.GetEntry(shownActionBarShortcutSetId);
            if (entry == null)
                return null;

            uint[] types   = [entry.ShortcutType00, entry.ShortcutType01, entry.ShortcutType02, entry.ShortcutType03, entry.ShortcutType04, entry.ShortcutType05,
                              entry.ShortcutType06, entry.ShortcutType07, entry.ShortcutType08, entry.ShortcutType09, entry.ShortcutType10, entry.ShortcutType11];
            uint[] objects = [entry.ObjectId00, entry.ObjectId01, entry.ObjectId02, entry.ObjectId03, entry.ObjectId04, entry.ObjectId05,
                              entry.ObjectId06, entry.ObjectId07, entry.ObjectId08, entry.ObjectId09, entry.ObjectId10, entry.ObjectId11];
            if (slot >= types.Length)
                return null;

            return (types[slot], objects[slot]);
        }

        /// <summary>
        /// Handle a pet command pressed on an action bar shown to the owner.
        /// </summary>
        public void HandleCommand(ShortcutSet shortcutSet, uint slot, bool associatedUnitOnly)
        {
            (uint Type, uint ObjectId)? shortcut = GetShownShortcut(shortcutSet, slot);
            if (shortcut == null || shortcut.Value.Type is not ShortcutTypeCommand and not ShortcutTypeUnitCommand)
            {
                log.Debug($"Player {owner.CharacterId} sent pet command for unknown shortcut {shortcutSet} {slot}.");
                return;
            }

            IEnumerable<ICombatPetEntity> pets = GetPets().Where(p => p.IsAlive && p.ValidStances != PetStance.None);
            if (associatedUnitOnly)
                pets = pets.Take(1);

            switch ((PetCommand)shortcut.Value.ObjectId)
            {
                case PetCommand.Attack:
                {
                    IUnitEntity target = owner.TargetGuid != null ? owner.GetVisible<IUnitEntity>(owner.TargetGuid.Value) : null;
                    if (target == null)
                        return;

                    foreach (ICombatPetEntity pet in pets)
                        pet.CommandAttack(target);
                    break;
                }
                case PetCommand.Stop:
                    foreach (ICombatPetEntity pet in pets)
                        pet.CommandStop();
                    break;
                case PetCommand.Follow:
                    foreach (ICombatPetEntity pet in pets)
                        pet.CommandFollow();
                    break;
                default:
                    log.Debug($"Player {owner.CharacterId} sent unhandled pet command {shortcut.Value.ObjectId}.");
                    break;
            }
        }

        /// <summary>
        /// Handle a spell pressed on an action bar shown to the owner (ClientGameCommand).
        /// </summary>
        public void HandleActionBarCast(ShortcutSet shortcutSet, uint slot, uint targetUnitId, Vector3 position)
        {
            (uint Type, uint ObjectId)? shortcut = GetShownShortcut(shortcutSet, slot);
            if (shortcut == null || shortcut.Value.Type is not ShortcutTypeSpell and not ShortcutTypeSpell2)
                return;

            Spell4Entry entry = gameTableManager.Spell4.GetEntry(shortcut.Value.ObjectId);
            if (entry == null)
                return;

            ISpellInfo spellInfo = spellInfoManager.GetSpellBaseInfo(entry.Spell4BaseIdBaseSpell)?.GetSpellInfo((byte)entry.TierIndex);
            if (spellInfo == null)
                return;

            owner.CastSpell(new SpellParameters
            {
                SpellInfo              = spellInfo,
                PrimaryTargetId        = targetUnitId,
                TargetPosition         = new Position(position),
                UserInitiatedSpellCast = true
            });
        }

        /// <summary>
        /// Move all pets to the supplied position and make them stay there.
        /// </summary>
        public void GoTo(Vector3 position)
        {
            foreach (ICombatPetEntity pet in GetPets().Where(p => p.IsAlive && p.ValidStances != PetStance.None))
                pet.CommandGoTo(position);
        }

        /// <summary>
        /// Add currently summoned pets to <see cref="ServerPlayerCreate"/>.
        /// </summary>
        public void AddToPlayerCreate(ServerPlayerCreate playerCreate)
        {
            foreach (ICombatPetEntity pet in GetPets())
            {
                playerCreate.Pets.Add(new ServerPlayerCreate.Pet
                {
                    Guid           = pet.Guid,
                    SummoningSpell = pet.SummonSpell4Id,
                    ValidStances   = (byte)pet.ValidStances,
                    Stance         = (byte)pet.Stance
                });
            }
        }
    }
}
