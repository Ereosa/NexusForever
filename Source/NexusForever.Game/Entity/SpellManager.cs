using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Info;
using NexusForever.Game.Prerequisite;
using NexusForever.Game.Spell;
using NexusForever.Game.Static.Abilities;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model;
using NexusForever.Network.World.Message.Model.Abilities;
using NexusForever.Network.World.Message.Model.Shared;
using NexusForever.Network.World.Message.Model.Spell;
using NexusForever.Network.World.Message.Static;
using NexusForever.Shared;
using NLog;

namespace NexusForever.Game.Entity
{
    public class SpellManager : ISpellManager
    {
        /// <summary>
        /// Determines which fields need saving for <see cref="ISpellManager"/> when being saved to the database.
        /// </summary>
        [Flags]
        public enum SpellManagerSaveMask
        {
            None            = 0x0000,
            ActiveActionSet = 0x0001,
            Innate          = 0x0002,
            BonusPoints     = 0x0004
        }

        // "Ability Tier Point Unlock" and "AMP Power Upgrade" items, the client tops out at 42 tier points (41 from levels) and 10 bonus AMP power
        public const byte MaxBonusAbilityPoints = ActionSet.MaxTierPoints - 41;
        public const byte MaxBonusAmpPower      = 10;

        public byte BonusAbilityPoints { get; private set; }
        public byte BonusAmpPower { get; private set; }

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Index of the active <see cref="IActionSet"/>.
        /// </summary>
        public byte ActiveActionSet
        {
            get => activeActionSet;
            private set
            {
                saveMask |= SpellManagerSaveMask.ActiveActionSet;
                activeActionSet = value;
            }
        }
        private byte activeActionSet;

        public byte InnateIndex
        {
            get => innateIndex;
            set
            {
                innateIndex = value;
                saveMask |= SpellManagerSaveMask.Innate;
            }
        }
        private byte innateIndex;

        private readonly IPlayer player;
        private ICharacterSpell continuousSpell;

        private readonly Dictionary<uint /*spell4BaseId*/, ICharacterSpell> spells = new();
        private readonly Dictionary<uint /*spell4Id*/, double /*cooldown*/> spellCooldowns = new();
        private readonly Dictionary<uint /*cooldownId*/, double /*cooldown*/> cooldownIds = new();
        private readonly Dictionary<uint /*globalCooldownEnum*/, double /*cooldown*/> globalSpellCooldowns = new();
        private uint maxGlobalSpellCooldownEnum = 3; // TODO: Read value from GameTables?

        private readonly IActionSet[] actionSets = new ActionSet[ActionSet.MaxActionSets];

        private SpellManagerSaveMask saveMask;

        /// <summary>
        /// Spell level prerequisites of known spells, keyed by Spell4Base id.
        /// </summary>
        private readonly Dictionary<uint, uint> spellPrerequisites = [];

        /// <summary>
        /// AMP passive spells currently cast on the player.
        /// </summary>
        private readonly HashSet<uint> ampPassiveSpell4Ids = [];

        private readonly Dictionary<Class, List<uint> /* spell4Id*/> classPassives = new Dictionary<Class, List<uint>>
        {
            { Class.Warrior, new List<uint> { 46707 } },
            { Class.Engineer, new List<uint> { 58450 } },
            { Class.Esper, new List<uint> { 75261 } },
            { Class.Medic, new List<uint> { 52081 } },
            { Class.Stalker, new List<uint> { 70634 } },
            { Class.Spellslinger, new List<uint> { 69704 } }
        };

        /// <summary>
        /// Create a new <see cref="ISpellManager"/> from existing <see cref="CharacterModel"/> database model.
        /// </summary>
        public SpellManager(IPlayer owner, CharacterModel model)
        {
            player = owner;

            foreach (CharacterSpellModel spellModel in model.Spell)
            {
                ISpellBaseInfo spellBaseInfo = LegacyServiceProvider.Provider.GetService<ISpellInfoManager>().GetSpellBaseInfo(spellModel.Spell4BaseId);
                IItem item = player.Inventory.SpellCreate(spellBaseInfo.Entry, ItemUpdateReason.NoReason);
                spells.Add(spellModel.Spell4BaseId, new CharacterSpell(owner, spellModel, spellBaseInfo, item));
            }

            for (uint i = 0; i <= maxGlobalSpellCooldownEnum; i++)
                globalSpellCooldowns.Add(i, 0d);

            GrantSpells();

            for (byte i = 0; i < ActionSet.MaxActionSets; i++)
            {
                actionSets[i] = new ActionSet(i, player);

                foreach (CharacterActionSetShortcutModel shortcutModel in model.ActionSetShortcut
                    .Where(c => c.SpecIndex == i))
                    actionSets[i].AddShortcut(shortcutModel);

                foreach (CharacterActionSetAmpModel ampModel in model.ActionSetAmp
                    .Where(c => c.SpecIndex == i))
                    actionSets[i].AddAmp(ampModel);
            }

            activeActionSet    = model.ActiveSpec;
            innateIndex        = model.InnateIndex;
            BonusAbilityPoints = model.BonusAbilityPoints;
            BonusAmpPower      = model.BonusAmpPower;
        }

        public void GrantSpells()
        {
            // TODO: TEMPORARY, this should eventually be used on level up
            foreach (SpellLevelEntry spellLevel in GameTableManager.Instance.SpellLevel.Entries
                .Where(s => s.ClassId == (byte)player.Class && s.CharacterLevel <= player.Level)
                .OrderBy(s => s.CharacterLevel))
            {
                Spell4Entry spell4Entry = GameTableManager.Instance.Spell4.GetEntry(spellLevel.Spell4Id);
                if (spell4Entry == null)
                    continue;

                // abilities with a prerequisite (eg: 42810 Repair Bot "Unlocked through AMPs") are known but can only be cast while it is met
                if (spellLevel.PrerequisiteId > 0)
                    spellPrerequisites[spell4Entry.Spell4BaseIdBaseSpell] = spellLevel.PrerequisiteId;

                if (GetSpell(spell4Entry.Spell4BaseIdBaseSpell) == null)
                    AddSpell(spell4Entry.Spell4BaseIdBaseSpell);

                // Add Pet Abilities if this Spell has a Pet summoned from it
                if (spell4Entry.Spell4IdPetSwitch > 0)
                {
                    Spell4Entry spell4PetEntry = GameTableManager.Instance.Spell4.GetEntry(spell4Entry.Spell4IdPetSwitch);
                    if (spell4PetEntry == null)
                        continue;

                    if (GetSpell(spell4PetEntry.Spell4BaseIdBaseSpell) == null)
                        AddSpell(spell4PetEntry.Spell4BaseIdBaseSpell);
                }

                if (spell4Entry.Spell4IdMechanicAlternateSpell > 0)
                {
                    Spell4Entry spell4AltEntry = GameTableManager.Instance.Spell4.GetEntry(spell4Entry.Spell4IdMechanicAlternateSpell);
                    if (spell4AltEntry == null)
                        continue;

                    if (GetSpell(spell4AltEntry.Spell4BaseIdBaseSpell) == null)
                        AddSpell(spell4AltEntry.Spell4BaseIdBaseSpell);
                }
            }

            // every character knows Sprint, Mount Sprint and Hoverboard Sprint, the client won't sprint unless the spell is known
            // tier 2 is the out of combat sprint the client casts from game formula 1227
            foreach (uint sprintBaseId in new uint[] { 1316u, 36122u, 36240u })
                if (GetSpell(sprintBaseId) == null)
                    AddSpell(sprintBaseId, 2);

            // "Recall - Transmat" (game formula 777) is known once the character binds to a Transmat Terminal,
            // the client's Recall button casts it from the spellbook and prerequisite 30314 "You don't have a bind point set" checks for it
            if (player.BindPoint != 0)
            {
                Spell4Entry recallEntry = GameTableManager.Instance.Spell4.GetEntry(GameTableManager.Instance.GameFormula.GetEntry(777)?.Dataint0 ?? 0u);
                if (recallEntry != null && GetSpell(recallEntry.Spell4BaseIdBaseSpell) == null)
                    AddSpell(recallEntry.Spell4BaseIdBaseSpell);
            }

            ClassEntry classEntry = GameTableManager.Instance.Class.GetEntry((byte)player.Class);
            foreach (uint classSpell in classEntry.Spell4IdInnateAbilityActive
                .Concat(classEntry.Spell4IdInnateAbilityPassive)
                .Concat(classEntry.Spell4IdAttackPrimary)
                .Concat(classEntry.Spell4IdAttackUnarmed))
            {
                Spell4Entry spell4Entry = GameTableManager.Instance.Spell4.GetEntry(classSpell);
                if (spell4Entry == null)
                    continue;

                if (GetSpell(spell4Entry.Spell4BaseIdBaseSpell) == null)
                    AddSpell(spell4Entry.Spell4BaseIdBaseSpell);
            }
        }

        public void Update(double lastTick)
        {
            // update global cooldowns
            foreach ((uint globalEnum, double cooldown) in globalSpellCooldowns.ToArray())
            {
                if (cooldown <= 0d)
                    continue;

                globalSpellCooldowns[globalEnum] = cooldown - lastTick;
            }

            // update spell cooldowns
            foreach ((uint spellId, double cooldown) in spellCooldowns.ToArray())
            {
                if (cooldown - lastTick <= 0d)
                {
                    spellCooldowns.Remove(spellId);
                    log.Trace($"Spell {spellId} cooldown has reset.");
                }
                else
                    spellCooldowns[spellId] = cooldown - lastTick;
            }

            // update cooldown id groups
            foreach ((uint cooldownId, double cooldown) in cooldownIds.ToArray())
            {
                if (cooldown - lastTick <= 0d)
                {
                    cooldownIds.Remove(cooldownId);
                    log.Trace($"Cooldown ID {cooldownId} has reset.");
                }
                else
                    cooldownIds[cooldownId] = cooldown - lastTick;
            }

            foreach (CharacterSpell unlockedSpell in spells.Values)
                unlockedSpell.Update(lastTick);

            if (continuousSpell != null && globalSpellCooldowns[continuousSpell.GlobalCooldownEnum] <= 0d && !player.IsCasting())
                continuousSpell?.SpellManagerCast();
        }

        public void Save(CharacterContext context)
        {
            if (saveMask != SpellManagerSaveMask.None)
            {
                // character is attached in Player::Save, this will only be local lookup
                CharacterModel character = context.Character.Find(player.CharacterId);
                EntityEntry<CharacterModel> entity = context.Entry(character);

                if ((saveMask & SpellManagerSaveMask.ActiveActionSet) != 0)
                {
                    character.ActiveSpec = ActiveActionSet;
                    entity.Property(p => p.ActiveSpec).IsModified = true;
                }

                if ((saveMask & SpellManagerSaveMask.Innate) != 0)
                {
                    character.InnateIndex = InnateIndex;
                    entity.Property(p => p.InnateIndex).IsModified = true;
                }

                if ((saveMask & SpellManagerSaveMask.BonusPoints) != 0)
                {
                    character.BonusAbilityPoints = BonusAbilityPoints;
                    entity.Property(p => p.BonusAbilityPoints).IsModified = true;
                    character.BonusAmpPower = BonusAmpPower;
                    entity.Property(p => p.BonusAmpPower).IsModified = true;
                }

                saveMask = SpellManagerSaveMask.None;
            }

            foreach (ICharacterSpell spell in spells.Values)
                spell.Save(context);

            foreach (IActionSet actionSet in actionSets)
                actionSet.Save(context);
        }

        /// <summary>
        /// Returns <see cref="ICharacterSpell"/> for an existing spell.
        /// </summary>
        public ICharacterSpell GetSpell(uint spell4BaseId)
        {
            return spells.TryGetValue(spell4BaseId, out ICharacterSpell spell) ? spell : null;
        }

        /// <summary>
        /// Add a new <see cref="ICharacterSpell"/> created from supplied spell base id and tier.
        /// </summary>
        public void AddSpell(uint spell4BaseId, byte tier = 1)
        {
            ISpellBaseInfo spellBaseInfo = LegacyServiceProvider.Provider.GetService<ISpellInfoManager>().GetSpellBaseInfo(spell4BaseId);
            if (spellBaseInfo == null)
                throw new ArgumentOutOfRangeException();

            ISpellInfo spellInfo = spellBaseInfo.GetSpellInfo(tier);
            if (spellInfo == null)
                throw new ArgumentOutOfRangeException();

            if (spells.ContainsKey(spell4BaseId))
                throw new InvalidOperationException();

            IItem item = player.Inventory.SpellCreate(spellBaseInfo.Entry, ItemUpdateReason.NoReason);

            var unlockedSpell = new CharacterSpell(player, spellBaseInfo, tier, item);
            if (!player.IsLoading)
            {
                player.Session.EnqueueMessageEncrypted(new ServerSpellUpdate
                {
                    Spell4BaseId = spell4BaseId,
                    TierIndex    = tier,
                    Activated    = tier > 0
                });
            }

            spells.Add(spellBaseInfo.Entry.Id, unlockedSpell);
        }

        /// <summary>
        /// Update existing <see cref="ICharacterSpell"/> with supplied tier. The base tier will be updated if no action set index is supplied.
        /// </summary>
        public void UpdateSpell(uint spell4BaseId, byte tier, byte? actionSetIndex)
        {
            ISpellBaseInfo spellBaseInfo = LegacyServiceProvider.Provider.GetService<ISpellInfoManager>().GetSpellBaseInfo(spell4BaseId);
            if (spellBaseInfo == null)
                throw new ArgumentOutOfRangeException();

            ISpellInfo spellInfo = spellBaseInfo.GetSpellInfo(tier);
            if (spellInfo == null)
                throw new ArgumentOutOfRangeException();

            ICharacterSpell spell = GetSpell(spell4BaseId);
            if (spell == null)
                throw new ArgumentOutOfRangeException();

            if (actionSetIndex == null)
                spell.Tier = tier;
            else
            {
                IActionSet actionSet = GetActionSet(actionSetIndex.Value);
                actionSet.UpdateSpellShortcut(spell4BaseId, tier);
            }

            if (!player.IsLoading)
            {
                player.Session.EnqueueMessageEncrypted(new ServerSpellUpdate
                {
                    Spell4BaseId = spell4BaseId,
                    TierIndex    = tier,
                    SpecIndex    = actionSetIndex ?? 0,
                    Activated    = tier > 0
                });
            }
        }

        /// <summary>
        /// Return the tier for supplied spell.
        /// This will either be the <see cref="IActionSetShortcut"/> tier if placed in the active <see cref="IActionSet"/> or base tier if not.
        /// </summary>
        public byte GetSpellTier(uint spell4BaseId)
        {
            ICharacterSpell spell = GetSpell(spell4BaseId);
            if (spell == null)
                throw new ArgumentException();

            IActionSet actionSet = GetActionSet(ActiveActionSet);
            IActionSetShortcut shortcut = actionSet.GetShortcut(ShortcutType.SpellbookItem, spell4BaseId);
            return shortcut?.Tier ?? spell.Tier;
        }

        public List<ICharacterSpell> GetPets()
        {
            return spells.Values
                .Where(s => s.BaseInfo.SpellType.Id == 27 ||
                            s.BaseInfo.SpellType.Id == 30 ||
                            s.BaseInfo.SpellType.Id == 104)
                .ToList();
        }

        /// <summary>
        /// Return remaining cooldown for supplied cooldown ID.
        /// </summary>
        public double GetSpellCooldownByCooldownId(uint spell4CooldownId)
        {
            return cooldownIds.TryGetValue(spell4CooldownId, out double cooldown) ? cooldown : 0u;
        }

        /// <summary>
        /// Update all spell cooldowns that share a given cooldown ID
        /// </summary>
        public void SetSpellCooldownByCooldownId(uint spell4CooldownId, double newCooldown)
        {
            if (newCooldown < 0d)
                throw new ArgumentOutOfRangeException();

            if (cooldownIds.TryGetValue(spell4CooldownId, out double cooldown))
                cooldownIds[spell4CooldownId] = newCooldown;
            else
                cooldownIds.TryAdd(spell4CooldownId, newCooldown);
        }

        /// <summary>
        /// Return spell cooldown for supplied spell id in seconds.
        /// </summary>
        public double GetSpellCooldown(uint spellId)
        {
            return spellCooldowns.TryGetValue(spellId, out double cooldown) ? cooldown : 0d;
        }

        /// <summary>
        /// Set spell cooldown in seconds for supplied spell id.
        /// </summary>
        private void _SetSpellCooldown(uint spell4Id, double cooldown, bool emit)
        {
            if (cooldown < 0d)
                throw new ArgumentOutOfRangeException();

            if (spellCooldowns.ContainsKey(spell4Id))
                spellCooldowns[spell4Id] = cooldown;
            else
                spellCooldowns.Add(spell4Id, cooldown);

            log.Trace($"Spell {spell4Id} cooldown set to {cooldown} seconds.");

            if (!player.IsLoading && emit)
            {
                player.Session.EnqueueMessageEncrypted(new ServerSpellCooldownStart
                {
                    Cooldown = new Cooldown
                    {
                        Type          = 1,
                        TypeId        = spell4Id,
                        SpellId       = spell4Id,
                        TimeRemaining = (uint)(cooldown * 1000u)
                    }
                });
            }
        }

        /// <summary>
        /// Set spell cooldown in seconds for supplied <see cref="ISpellInfo"/>.
        /// </summary>
        public void SetSpellCooldown(ISpellInfo spellInfo, double cooldown, bool emit = false)
        {
            if (cooldown < 0d)
                throw new ArgumentOutOfRangeException();

            _SetSpellCooldown(spellInfo.Entry.Id, cooldown, emit);

            // mechanic alternates (Spellslinger Charged Shot and its surged form) share the cooldown and the action bar slot,
            // the client only shows a cooldown on the spell id it is told about
            foreach (uint alternateSpell4Id in GetMechanicAlternateSpellIds(spellInfo.Entry).ToList())
                _SetSpellCooldown(alternateSpell4Id, cooldown, emit);

            foreach (SpellCoolDownEntry coolDownEntry in spellInfo.Cooldowns)
                SetSpellCooldownByCooldownId(coolDownEntry.Id, cooldown);
        }

        private IEnumerable<uint> GetMechanicAlternateSpellIds(Spell4Entry entry)
        {
            if (entry.Spell4IdMechanicAlternateSpell > 0)
                yield return entry.Spell4IdMechanicAlternateSpell;

            foreach (ICharacterSpell characterSpell in spells.Values)
            {
                if (characterSpell.SpellInfo.Entry.Spell4IdMechanicAlternateSpell == entry.Id)
                    yield return characterSpell.SpellInfo.Entry.Id;
            }
        }

        /// <summary>
        /// Set spell cooldown in seconds for supplied spell id.
        /// </summary>
        public void SetSpellCooldown(uint spell4Id, double cooldown, bool emit = false)
        {
            if (cooldown < 0d)
                throw new ArgumentOutOfRangeException();

            Spell4Entry entry = GameTableManager.Instance.Spell4.GetEntry(spell4Id);
            if (entry == null)
                throw new InvalidOperationException($"Spell4 with ID ({spell4Id}) does not exist.");

            ISpellBaseInfo baseInfo = LegacyServiceProvider.Provider.GetService<ISpellInfoManager>().GetSpellBaseInfo(entry.Spell4BaseIdBaseSpell);
            if (baseInfo == null)
                throw new InvalidOperationException($"BaseInfo with ID ({entry.Spell4BaseIdBaseSpell}) does not exist.");

            SetSpellCooldown(baseInfo.GetSpellInfo((byte)entry.TierIndex), cooldown, emit);
        }

        /// <summary>
        /// Update all spell cooldowns that share a given group ID
        /// </summary>
        public void SetSpellCooldownByGroupId(uint groupId, double cooldown)
        {
            if (cooldown < 0d)
                throw new ArgumentOutOfRangeException();

            foreach (uint spell4 in spellCooldowns.Keys.ToList())
            {
                Spell4Entry spell4Entry = GameTableManager.Instance.Spell4.GetEntry(spell4);
                if (spell4Entry == null)
                    throw new InvalidOperationException($"Spell4 with ID ({spell4}) does not exist.");

                if (spell4Entry.Spell4GroupListId == 0u)
                    continue;

                Spell4GroupListEntry spell4GroupListEntry = GameTableManager.Instance.Spell4GroupList.GetEntry(spell4Entry.Spell4GroupListId);
                if (spell4GroupListEntry == null)
                    throw new InvalidOperationException($"Spell4 Group List with ID ({spell4Entry.Spell4GroupListId}) does not exist.");

                if (spell4GroupListEntry.SpellGroupIds.Contains(groupId))
                    SetSpellCooldown(spell4, cooldown);
            }
        }

        /// <summary>
        /// Update all spell cooldowns that share a base spell ID
        /// </summary>
        public void SetSpellCooldownByBaseSpell(uint spell4BaseId, uint type, double cooldown)
        {
            ISpellBaseInfo baseSpellInfo = LegacyServiceProvider.Provider.GetService<ISpellInfoManager>().GetSpellBaseInfo(spell4BaseId);
            if (baseSpellInfo == null)
                throw new ArgumentNullException();

            foreach (ISpellInfo spellInfo in baseSpellInfo)
            {
                if (spellCooldowns.TryGetValue(spellInfo.Entry.Id, out double currentCd))
                {
                    if (type == 3)
                        SetSpellCooldown(spellInfo.Entry.Id, cooldown / 1000d);
                    else
                        SetSpellCooldown(spellInfo.Entry.Id, currentCd * cooldown);
                }
            }
        }

        public void ResetAllSpellCooldowns()
        {
            foreach (uint spell4Id in spellCooldowns.Keys.ToList())
                SetSpellCooldown(spell4Id, 0d, true);
        }

        public double GetGlobalSpellCooldown(uint globalEnum)
        {
            return globalSpellCooldowns[globalEnum];
        }

        public void SetGlobalSpellCooldown(uint globalEnum, double cooldown)
        {
            globalSpellCooldowns[globalEnum] = cooldown;
            log.Trace($"Global spell cooldown {globalEnum} set to {cooldown} seconds.");
        }

        /// <summary>
        /// Return <see cref="IActionSet"/> at supplied index.
        /// </summary>
        public IActionSet GetActionSet(byte actionSetIndex)
        {
            if (actionSetIndex >= ActionSet.MaxActionSets)
                throw new ArgumentOutOfRangeException();

            return actionSets[actionSetIndex];
        }

        /// <summary>
        /// Update active <see cref="IActionSet"/> with supplied index, returned <see cref="SpecError"/> is sent to the client.
        /// </summary>
        public SpecError SetActiveActionSet(byte value)
        {
            if (value >= ActionSet.MaxActionSets)
                return SpecError.InvalidIndex;

            if (value == ActiveActionSet)
                return SpecError.NoChange;

            // TODO: handle other errors

            ActiveActionSet = value;
            UpdateAmpPassives();
            return SpecError.Ok;
        }

        /// <summary>
        /// Returns if the spell level prerequisite of the supplied spell is met.
        /// </summary>
        public bool CanCastSpell(uint spell4BaseId)
        {
            if (!spellPrerequisites.TryGetValue(spell4BaseId, out uint prerequisiteId))
                return true;

            return PrerequisiteManager.Instance.Meets(player, prerequisiteId);
        }

        /// <summary>
        /// Returns the total AMP power available to the player.
        /// </summary>
        /// <remarks>
        /// Matches the client: game formula 942 grants Dataint01 power at level Dataint0 and Datafloat0 power each level after.
        /// </remarks>
        public uint GetAmpPower()
        {
            GameFormulaEntry formula = GameTableManager.Instance.GameFormula.GetEntry(942);
            uint startLevel  = formula?.Dataint0 ?? 10u;
            uint startPower  = formula?.Dataint01 ?? 4u;
            float levelPower = formula?.Datafloat0 ?? 1f;

            if (player.Level < startLevel)
                return BonusAmpPower;

            return startPower + (uint)((player.Level - startLevel) * levelPower) + BonusAmpPower;
        }

        /// <summary>
        /// Add permanent bonus ability tier points, returns false if the character already has the maximum.
        /// </summary>
        public bool AddBonusAbilityPoints(uint amount)
        {
            if (BonusAbilityPoints >= MaxBonusAbilityPoints)
                return false;

            BonusAbilityPoints = (byte)Math.Min(BonusAbilityPoints + amount, MaxBonusAbilityPoints);
            saveMask |= SpellManagerSaveMask.BonusPoints;
            SendServerAbilityPoints();
            return true;
        }

        /// <summary>
        /// Add permanent bonus AMP power, returns false if the character already has the maximum.
        /// </summary>
        public bool AddBonusAmpPower(uint amount)
        {
            if (BonusAmpPower >= MaxBonusAmpPower)
                return false;

            BonusAmpPower = (byte)Math.Min(BonusAmpPower + amount, MaxBonusAmpPower);
            saveMask |= SpellManagerSaveMask.BonusPoints;
            SendServerAmpPower();
            return true;
        }

        private void SendServerAmpPower()
        {
            player.Session.EnqueueMessageEncrypted(new ServerAmpPowerUpdate
            {
                BonusPower = BonusAmpPower
            });
        }

        /// <summary>
        /// Add the supplied AMPs to the <see cref="IActionSet"/>, AMPs already in the action set are ignored.
        /// </summary>
        /// <remarks>
        /// The client sends the full list of AMPs for the action set, existing and newly selected.
        /// </remarks>
        public LimitedActionSetResult AddAmps(byte actionSetIndex, IEnumerable<ushort> ampIds)
        {
            if (actionSetIndex >= ActionSet.MaxActionSets)
                return LimitedActionSetResult.InvalidSpecIndex;

            IActionSet actionSet = actionSets[actionSetIndex];

            // prerequisite AMPs and lower category tiers first, the client can select them together
            List<EldanAugmentationEntry> newAmps = ampIds
                .Distinct()
                .Where(id => actionSet.GetAmp(id) == null)
                .Select(id => GameTableManager.Instance.EldanAugmentation.GetEntry(id))
                .ToList();

            if (newAmps.Any(e => e == null))
                return LimitedActionSetResult.EldanAugmentationInvalidId;

            LimitedActionSetResult result = LimitedActionSetResult.Ok;
            while (newAmps.Count > 0)
            {
                EldanAugmentationEntry entry = newAmps
                    .OrderBy(e => e.CategoryTier)
                    .FirstOrDefault(e => e.EldanAugmentationIdRequired == 0 || actionSet.GetAmp((ushort)e.EldanAugmentationIdRequired) != null)
                    ?? newAmps[0];
                newAmps.Remove(entry);

                LimitedActionSetResult ampResult = CanAddAmp(actionSet, entry);
                if (ampResult != LimitedActionSetResult.Ok)
                {
                    log.Debug($"Player {player.CharacterId} failed to add AMP {entry.Id} to action set {actionSetIndex}: {ampResult}.");
                    result = ampResult;
                    continue;
                }

                actionSet.AddAmp((ushort)entry.Id);
            }

            if (actionSetIndex == ActiveActionSet)
                UpdateAmpPassives();

            return result;
        }

        private LimitedActionSetResult CanAddAmp(IActionSet actionSet, EldanAugmentationEntry entry)
        {
            if (entry.ClassId != (uint)player.Class)
                return LimitedActionSetResult.UnknownClassId;

            EldanAugmentationCategoryEntry category = GameTableManager.Instance.EldanAugmentationCategory.GetEntry(entry.EldanAugmentationCategoryId);
            if (category == null)
                return LimitedActionSetResult.EldanAugmentationInvalidCategoryId;

            if (entry.CategoryTier > 2)
                return LimitedActionSetResult.EldanAugmentationInvalidCategoryTier;

            // eg: Critical Hit II requires Critical Hit I
            if (entry.EldanAugmentationIdRequired != 0 && actionSet.GetAmp((ushort)entry.EldanAugmentationIdRequired) == null)
                return LimitedActionSetResult.EldanAugmentationInvalidSeries;

            if (entry.Item2IdUnlock != 0)
                return LimitedActionSetResult.EldanAugmentationLockedInlaid;

            if (!IsAmpCategoryTierUnlocked(actionSet, entry, category))
                return LimitedActionSetResult.EldanAugmentationLockedCategoryTier;

            uint spent = (uint)actionSet.Amps.Sum(a => a.Entry.PowerCost);
            if (spent + entry.PowerCost > GetAmpPower())
                return LimitedActionSetResult.EldanAugmentationNotEnoughPower;

            return LimitedActionSetResult.Ok;
        }

        /// <summary>
        /// Returns if the category tier of the supplied AMP is unlocked by power spent in the lower tiers of the category.
        /// </summary>
        /// <remarks>
        /// Tier 2 needs Tier2CostAmount power and tier 3 needs Tier3CostAmount power (3 and 11 for every category).
        /// </remarks>
        private static bool IsAmpCategoryTierUnlocked(IActionSet actionSet, EldanAugmentationEntry entry, EldanAugmentationCategoryEntry category)
        {
            if (entry.CategoryTier == 0)
                return true;

            uint unlockCategoryId = category.EldanAugmentationCategoryIdTier2Category00 != 0 ? category.EldanAugmentationCategoryIdTier2Category00 : category.Id;
            uint required = entry.CategoryTier == 1 ? category.Tier2CostAmount00 : category.Tier3CostAmount00;

            uint power = (uint)actionSet.Amps
                .Where(a => a.Entry.EldanAugmentationCategoryId == unlockCategoryId && a.Entry.CategoryTier < entry.CategoryTier && a.Entry.Id != entry.Id)
                .Sum(a => a.Entry.PowerCost);

            return power >= required;
        }

        /// <summary>
        /// Remove AMPs from the <see cref="IActionSet"/>, AMPs that are no longer valid are also removed.
        /// </summary>
        public void RemoveAmps(byte actionSetIndex, AmpRespecType type, uint value)
        {
            if (actionSetIndex >= ActionSet.MaxActionSets)
                return;

            IActionSet actionSet = actionSets[actionSetIndex];
            actionSet.RemoveAmp(type, value);

            // AMPs in a series or higher tier lose their requirement when lower AMPs are removed
            bool removed;
            do
            {
                removed = false;
                foreach (IActionSetAmp amp in actionSet.Amps.OrderByDescending(a => a.Entry.CategoryTier).ToList())
                {
                    EldanAugmentationCategoryEntry category = GameTableManager.Instance.EldanAugmentationCategory.GetEntry(amp.Entry.EldanAugmentationCategoryId);
                    bool seriesMet = amp.Entry.EldanAugmentationIdRequired == 0 || actionSet.GetAmp((ushort)amp.Entry.EldanAugmentationIdRequired) != null;
                    if (seriesMet && category != null && IsAmpCategoryTierUnlocked(actionSet, amp.Entry, category))
                        continue;

                    actionSet.RemoveAmp(AmpRespecType.Single, amp.Entry.Id);
                    removed = true;
                }
            }
            while (removed);

            if (actionSetIndex == ActiveActionSet)
                UpdateAmpPassives();
        }

        /// <summary>
        /// Cast the passive spells of the AMPs in the active <see cref="IActionSet"/> and remove those of AMPs no longer active.
        /// </summary>
        public void UpdateAmpPassives()
        {
            var activeSpell4Ids = actionSets[ActiveActionSet].Amps
                .Select(a => a.Entry.Spell4IdAugment)
                .Where(id => id != 0u)
                .ToHashSet();

            List<uint> removedSpell4Ids = ampPassiveSpell4Ids.Except(activeSpell4Ids).ToList();
            if (removedSpell4Ids.Count > 0)
            {
                player.FinishSpells(s => removedSpell4Ids.Contains(s.Spell4Id));
                foreach (uint spell4Id in removedSpell4Ids)
                    ampPassiveSpell4Ids.Remove(spell4Id);
            }

            if (player.Map == null)
                return;

            foreach (uint spell4Id in activeSpell4Ids)
            {
                if (ampPassiveSpell4Ids.Contains(spell4Id) && player.HasSpell(s => s.Spell4Id == spell4Id && !s.IsFinished, out ISpell _))
                    continue;

                if (GameTableManager.Instance.Spell4.GetEntry(spell4Id) == null)
                    continue;

                player.CastSpell(spell4Id, new SpellParameters
                {
                    UserInitiatedSpellCast = false
                });
                ampPassiveSpell4Ids.Add(spell4Id);
            }
        }

        /// <summary>
        /// Update active Innate Ability with supplied index.
        /// </summary>
        public void SetInnate(byte index)
        {
            ClassEntry entry = GameTableManager.Instance.Class.GetEntry((ulong)player.Class);
            if (entry == null)
                throw new InvalidOperationException($"Player Class does not have an entry: {player.Class}");

            if (entry.Spell4IdInnateAbilityActive[index] == 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            uint innateActiveBaseId = GameTableManager.Instance.Spell4.GetEntry(entry.Spell4IdInnateAbilityActive[index]).Spell4BaseIdBaseSpell;
            if (innateActiveBaseId == 0)
                throw new InvalidOperationException("Selected Innate ability does not have an associated Spell4 Entry.");

            ICharacterSpell innateActive = GetSpell(innateActiveBaseId);
            if (innateActive == null)
                throw new InvalidOperationException($"Player does not have spell with a Base ID of {innateActiveBaseId}");

            if (entry.PrerequisiteIdInnateAbility[index] > 0)
                if (!PrerequisiteManager.Instance.Meets(player, entry.PrerequisiteIdInnateAbility[index]))
                    return;

            // TODO: Cast Innate Passives

            InnateIndex = index;
        }

        /// <summary>
        /// This uses passive On Start abilities that were caught in sniffs. Provides a lot of Procs and hidden class functionality.
        /// </summary>
        private void CastClassPassives()
        {
            if (classPassives.TryGetValue(player.Class, out List<uint> classPassiveSpells))
            {
                foreach (uint spell4Id in classPassiveSpells)
                {
                    if (player.HasSpell(spell4Id, out ISpell currentClassPassive))
                        continue;

                    player.CastSpell(spell4Id, new SpellParameters
                    {
                        UserInitiatedSpellCast = false
                    });
                }
            }
        }

        /// <summary>
        /// Set the supplied spell as the spell to Continuously Cast when GCD expires. Should only be called by a <see cref="ICharacterSpell"/>.
        /// </summary>
        public void SetAsContinuousCast(ICharacterSpell spell)
        {
            continuousSpell = spell;
        }

        public void SendInitialPackets()
        {
            SendServerAbilities();
            SendServerAbilityList();
            SendServerAbilityPoints();
            SendServerAmpPower();
            SendServerActionSets();
            SendServerAmpLists();
            SendServerPlayerInnate();
            // TODO: Cast Innate Spells
            CastClassPassives();
            UpdateAmpPassives();

            player.Session.EnqueueMessageEncrypted(new ServerCooldownList
            {
                Cooldowns = spellCooldowns.Select(c => new Cooldown
                {
                    Type          = 1,
                    SpellId       = c.Key,
                    TypeId        = c.Key,
                    TimeRemaining = (uint)(c.Value * 1000u)
                }).ToList()
            });
        }

        private void SendServerAbilities()
        {
            foreach (IItem spell in player.Inventory
                .Where(b => b.Location == InventoryLocation.Ability)
                .SelectMany(s => s))
            {
                player.Session.EnqueueMessageEncrypted(new ServerItemAdd
                {
                    InventoryItem = new InventoryItem
                    {
                        Item   = spell.Build(),
                        Reason = ItemUpdateReason.NoReason
                    }
                });
            }
        }

        private void SendServerAbilityList()
        {
            var serverAbilities = new ServerAbilities();
            foreach ((uint spell4BaseId, ICharacterSpell spell) in spells)
            {
                ISpellBaseInfo spellBaseInfo = LegacyServiceProvider.Provider.GetService<ISpellInfoManager>().GetSpellBaseInfo(spell4BaseId);
                if (spellBaseInfo == null)
                    continue;

                for (byte i = 0; i < ActionSet.MaxActionSets; i++)
                {
                    IActionSetShortcut shortcut = actionSets[i].GetShortcut(ShortcutType.SpellbookItem, spell4BaseId);
                    serverAbilities.Spells.Add(new ServerAbilities.Spell
                    {
                        Spell4BaseId      = spell4BaseId,
                        TierIndexAchieved = shortcut?.Tier ?? spell.Tier,
                        SpecIndex         = i
                    });

                    // class ability
                    if (spellBaseInfo.SpellType.Id != 5)
                        break;
                }
            }

            player.Session.EnqueueMessageEncrypted(serverAbilities);
        }

        public void SendServerAbilityPoints()
        {
            uint total = GetAbilityPoints();
            uint spent = GetSpentAbilityPoints(ActiveActionSet);

            player.Session.EnqueueMessageEncrypted(new ServerAbilityPoints
            {
                AbilityPoints      = total > spent ? total - spent : 0u,
                TotalAbilityPoints = total
            });
        }

        /// <summary>
        /// Returns the total ability tier points for the player's level.
        /// </summary>
        /// <remarks>
        /// XpPerLevel grants 1 point a level from level 10, 41 at level 50.
        /// </remarks>
        public uint GetAbilityPoints()
        {
            uint points = 0u;
            for (uint level = 1u; level <= player.Level; level++)
                points += GameTableManager.Instance.XpPerLevel.GetEntry(level)?.AbilityPointsPerLevel ?? 0u;

            return points + BonusAbilityPoints;
        }

        private uint GetSpentAbilityPoints(byte actionSetIndex)
        {
            return (uint)(ActionSet.MaxTierPoints - actionSets[actionSetIndex].TierPoints);
        }

        /// <summary>
        /// Returns if the supplied spell can be set to the supplied tier in the <see cref="IActionSet"/>, checking the tier level requirement and available ability points.
        /// </summary>
        public bool CanSetSpellTier(byte actionSetIndex, uint spell4BaseId, byte tier)
        {
            if (actionSetIndex >= ActionSet.MaxActionSets || tier < 1 || tier > ActionSet.MaxTier)
                return false;

            // Spell4TierRequirements: tier 2 at level 10, tier 3 at level 15 ... tier 9 at level 45
            Spell4TierRequirementsEntry requirement = GameTableManager.Instance.Spell4TierRequirements.Entries
                .FirstOrDefault(e => e != null && e.TierIndex == tier);
            if (requirement != null && player.Level < requirement.LevelRequirement)
                return false;

            IActionSetShortcut shortcut = actionSets[actionSetIndex].GetShortcut(ShortcutType.SpellbookItem, spell4BaseId);
            int currentCost = shortcut != null ? ActionSet.GetTierCost(shortcut.Tier) : 0;
            int cost = ActionSet.GetTierCost(tier) - currentCost;

            return GetSpentAbilityPoints(actionSetIndex) + cost <= GetAbilityPoints();
        }

        private void SendServerActionSets()
        {
            for (byte i = 0; i < ActionSet.MaxActionSets; i++)
            {
                IActionSet actionSet = GetActionSet(i);
                player.Session.EnqueueMessageEncrypted(actionSet.BuildServerActionSet());
            }
        }

        private void SendServerAmpLists()
        {
            for (byte i = 0; i < ActionSet.MaxActionSets; i++)
            {
                IActionSet actionSet = GetActionSet(i);
                player.Session.EnqueueMessageEncrypted(actionSet.BuildServerAmpList());
            }
        }

        private void SendServerPlayerInnate()
        {
            player.Session.EnqueueMessageEncrypted(new ServerStanceChanged
            {
                InnateIndex = InnateIndex
            });
        }
    }
}
