using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Combat;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Movement;
using NexusForever.Game.Abstract.Entity.Stat;
using NexusForever.Game.Abstract.Map.Search;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Info;
using NexusForever.Game.Abstract.Spell.Proc;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Combat;
using NexusForever.Game.Combat.CrowdControl;
using NexusForever.Game.Spell.Proc;
using NexusForever.Game.Static;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Game.Static.Quest;
using NexusForever.Game.Static.Reputation;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model;
using NexusForever.Network.World.Message.Model.Shared;
using NexusForever.Network.World.Message.Static;
using NexusForever.Script;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Collection;
using NexusForever.Shared;
using NexusForever.Network.World.Message.Model.Spell;

namespace NexusForever.Game.Entity
{
    public abstract class UnitEntity : WorldEntity, IUnitEntity
    {
        public float HitRadius { get; protected set; } = 1f;

        /// <summary>
        /// Guid of the <see cref="IUnitEntity"/> currently targeted.
        /// </summary>
        public uint? TargetGuid { get; private set; }

        /// <summary>
        /// Determines whether or not this <see cref="IUnitEntity"/> is alive.
        /// </summary>
        public bool IsAlive => Health > 0u && deathState == null;

        protected EntityDeathState? DeathState
        {
            get => deathState;
            set
            {
                deathState = value;

                if (deathState is null or EntityDeathState.JustDied)
                {
                    EnqueueToVisible(new ServerEntityDeathState
                    {
                        UnitId    = Guid,
                        Dead      = !IsAlive,
                        Reason    = 0, // client does nothing with this value
                        RezHealth = IsAlive ? Health : 0u
                    }, true);
                }
            }
        }

        private EntityDeathState? deathState;

        /// <summary>
        /// Determines whether or not this <see cref="IUnitEntity"/> is in combat.
        /// </summary>
        public bool InCombat
        {
            get => inCombat;
            private set
            {
                if (inCombat == value)
                    return;

                inCombat = value;

                EnqueueToVisible(new ServerUnitEnteredCombat
                {
                    UnitId   = Guid,
                    InCombat = value
                }, true);
            }
        }

        private bool inCombat;

        public IThreatManager ThreatManager { get; private set; }
        public IProcManager ProcManager { get; private set; }
        public ICrowdControlManager CrowdControlManager { get; private set; }

        private IStatUpdateManager statUpdateManager;

        private readonly List<ISpell> pendingSpells = [];
        private readonly Dictionary<uint, ISpell> spells = [];

        private readonly Dictionary<Property, Dictionary</*spell4Id*/uint, ISpellPropertyModifier>> spellProperties = [];

        #region Dependency Injection

        private readonly ISpellFactory spellFactory;

        public UnitEntity(IMovementManager movementManager,
            IEntitySummonFactory entitySummonFactory,
            IStatUpdateManager statUpdateManager,
            ISpellFactory spellFactory)
            : base(movementManager, entitySummonFactory)
        {
            this.statUpdateManager = statUpdateManager;
            this.spellFactory      = spellFactory;

            // TODO: find a better way to initialise unit managers...
            ThreatManager = new ThreatManager(this);
            ProcManager   = LegacyServiceProvider.Provider.GetService<IProcManager>();
            ProcManager.Initialise(this);
            CrowdControlManager = LegacyServiceProvider.Provider.GetService<ICrowdControlManager>();
            CrowdControlManager.Initialise(this);

            InitialiseHitRadius();
        }

        #endregion

        public override void Dispose()
        {
            base.Dispose();

            foreach (ISpell spell in spells.Values)
                spell.Dispose();
        }

        private void InitialiseHitRadius()
        {
            if (CreatureInfo?.ModelEntry != null)
                HitRadius = CreatureInfo.ModelEntry.HitRadius * CreatureInfo.Entry.ModelScale;
        }

        public override void Update(double lastTick)
        {
            base.Update(lastTick);

            // new spells can be added during updates
            // we need a pending queue to prevent issues with iterating over the spell dictionary
            foreach (ISpell spell in pendingSpells)
                spells.Add(spell.CastingId, spell);

            pendingSpells.Clear();

            foreach (ISpell spell in spells.Values)
            {
                spell.Update(lastTick);
                spell.LateUpdate(lastTick);
                if (spell.IsFinished)
                    spells.Remove(spell.CastingId);
            }

            statUpdateManager.Update(lastTick);

            ProcManager.Update(lastTick);
        }

        public override ServerEntityCreate BuildCreatePacket(bool initialCommands)
        {
            ServerEntityCreate entityCreate = base.BuildCreatePacket(initialCommands);

            foreach (ISpell spell in spells.Values)
            {
                SpellInit spellInit = spell.BuildSpellInit();
                entityCreate.SpellInitData.Add(spellInit);
            }

            // TODO
            // entityCreate.CurrentSpellUniqueId

            return entityCreate;
        }

        /// <summary>
        /// Initialise <see cref="IScriptCollection"/> for <see cref="IUnitEntity"/>.
        /// </summary>
        protected override void InitialiseScriptCollection(List<string> names)
        {
            scriptCollection = ScriptManager.Instance.InitialiseOwnedCollection<IUnitEntity>(this);
            ScriptManager.Instance.InitialiseEntityScripts<IUnitEntity>(scriptCollection, this, names);
        }

        /// <summary>
        /// Remove tracked <see cref="IGridEntity"/> that is no longer in vision range.
        /// </summary>
        protected override void RemoveVisible(IGridEntity entity)
        {
            if (entity.Guid == TargetGuid)
                SetTarget((IWorldEntity)null);

            ThreatManager.RemoveHostile(entity.Guid);

            base.RemoveVisible(entity);
        }

        /// <summary>
        /// Add a <see cref="Property"/> modifier given a Spell4Id and <see cref="ISpellPropertyModifier"/> instance.
        /// </summary>
        public void AddSpellModifierProperty(ISpellPropertyModifier spellModifier, uint spell4Id)
        {
            if (spellProperties.TryGetValue(spellModifier.Property, out Dictionary<uint, ISpellPropertyModifier> spellDict))
            {
                if (spellDict.ContainsKey(spell4Id))
                    spellDict[spell4Id] = spellModifier;
                else
                    spellDict.Add(spell4Id, spellModifier);
            }
            else
            {
                spellProperties.Add(spellModifier.Property, new Dictionary<uint, ISpellPropertyModifier>
                {
                    { spell4Id, spellModifier }
                });
            }

            CalculateProperty(spellModifier.Property);
        }

        /// <summary>
        /// Remove a <see cref="Property"/> modifier by a Spell that is currently affecting this <see cref="IUnitEntity"/>.
        /// </summary>
        public void RemoveSpellProperty(Property property, uint spell4Id)
        {
            if (spellProperties.TryGetValue(property, out Dictionary<uint, ISpellPropertyModifier> spellDict))
                spellDict.Remove(spell4Id);

            CalculateProperty(property);
        }

        /// <summary>
        /// Return all <see cref="IPropertyModifier"/> for this <see cref="IUnitEntity"/>'s <see cref="Property"/>
        /// </summary>
        private IEnumerable<ISpellPropertyModifier> GetSpellPropertyModifiers(Property property)
        {
            return spellProperties.ContainsKey(property) ? spellProperties[property].Values : Enumerable.Empty<ISpellPropertyModifier>();
        }

        protected override void CalculatePropertyValue(IPropertyValue propertyValue)
        {
            base.CalculatePropertyValue(propertyValue);

            // Run through spell adjustments first because they could adjust base properties
            // dataBits01 appears to be some form of Priority or Math Operator
            foreach (ISpellPropertyModifier spellModifier in GetSpellPropertyModifiers(propertyValue.Property)
                .OrderByDescending(s => s.Priority))
            {
                foreach (IPropertyModifier alteration in spellModifier.Alterations)
                {
                    // TODO: Add checks to ensure we're not modifying FlatValue and Percentage in the same effect?
                    switch (alteration.ModType)
                    {
                        case ModType.FlatValue:
                        case ModType.LevelScale:
                            propertyValue.Value += alteration.GetValue(Level);
                            break;
                        case ModType.Percentage:
                            propertyValue.Value *= alteration.GetValue();
                            break;
                    }
                }
            }

            if (propertyValue.Property == Property.InterruptArmorThreshold)
                propertyValue.Value += CrowdControlManager.GetTemporaryInterruptArmour();
        }

        /// Checks if this <see cref="IUnitEntity"/> is currently casting a spell.
        /// </summary>
        /// <returns></returns>
        public bool IsCasting()
        {
            foreach (ISpell spell in spells.Values)
                if (spell.IsCasting)
                    return true;

            return false;
        }

        /// <summary>
        /// Return <see cref="ISpell"/> with the supplied casting id.
        /// </summary>
        public ISpell GetSpell(uint castingId)
        {
            return spells.TryGetValue(castingId, out ISpell spell) ? spell : null;
        }

        /// <summary>
        /// Return <see cref="ISpell"/> with the supplied spell id.
        /// </summary>
        public ISpell GetSpellBySpellId(uint spellId)
        {
            // a spell can be active more than once (stacks, recasts), prefer one that is still running
            return spells.Values.Where(s => s.Spell4Id == spellId).OrderBy(s => s.IsFinished).FirstOrDefault();
        }

        /// <summary>
        /// Return <see cref="ISpell"/> with the supplied base spell id.
        /// </summary>
        public ISpell GetSpellByBaseSpellId(uint baseSpellId)
        {
            return spells.Values.Where(s => s.Parameters.SpellInfo.BaseInfo.Entry.Id == baseSpellId).OrderBy(s => s.IsFinished).FirstOrDefault();
        }

        /// <summary>
        /// Return a collection of <see cref="ISpell"/> that are part of the supplied spell group id.
        /// </summary>
        public IEnumerable<ISpell> GetSpellsByGroupId(uint spellGroupId)
        {
            foreach (ISpell spell in spells.Values)
                if (spell.Parameters.SpellInfo.SpellGroups.Contains(spellGroupId))
                    yield return spell;
        }

        /// <summary>
        /// Return a collection of <see cref="ISpell"/> that are applying the supplied <see cref="SpellEffectType"/> to entity.
        /// </summary>
        public IEnumerable<ISpell> GetSpellsByEffect(SpellEffectType type)
        {
            foreach (ISpell spell in spells.Values)
            {
                ISpellTargetInfo target = spell.GetTarget(this);
                if (target == null)
                    continue;

                if (target.GetEffectsByType(type).Any())
                    yield return spell;
            }
        }

        /// <summary>
        /// Check if this <see cref="IUnitEntity"/> has a spell active with the provided <see cref="Spell4Entry"/> Id
        /// </summary>
        public bool HasSpell(uint spell4Id, out ISpell spell, bool isCasting = false)
        {
            spell = spells.Values.FirstOrDefault(i => i.IsCasting == isCasting && !i.IsFinished && i.Spell4Id == spell4Id);
            return spell != null;
        }

        /// <summary>
        /// Check if this <see cref="IUnitEntity"/> has a spell active with the provided <see cref="CastMethod"/>
        /// </summary>
        public bool HasSpell(CastMethod castMethod, out ISpell spell)
        {
            spell = spells.Values.FirstOrDefault(i => !i.IsCasting && !i.IsFinished && i.CastMethod == castMethod);
            return spell != null;
        }

        /// <summary>
        /// Check if this <see cref="IUnitEntity"/> has a spell active with the provided <see cref="Func"/> predicate.
        /// </summary>
        public bool HasSpell(Func<ISpell, bool> predicate, out ISpell spell)
        {
            spell = spells.Values.FirstOrDefault(predicate);
            return spell != null;
        }

        /// <summary>
        /// Cast a <see cref="ISpell"/> with the supplied spell id and <see cref="ISpellParameters"/>.
        /// </summary>
        public void CastSpell<T>(T spell4Id, ISpellParameters parameters) where T : Enum
        {
            CastSpell(spell4Id.As<T, uint>(), parameters);
        }

        /// <summary>
        /// Cast a <see cref="ISpell"/> with the supplied spell id and <see cref="ISpellParameters"/>.
        /// </summary>
        public void CastSpell(uint spell4Id, ISpellParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException();

            Spell4Entry spell4Entry = GameTableManager.Instance.Spell4.GetEntry(spell4Id);
            if (spell4Entry == null)
                throw new ArgumentOutOfRangeException();

            CastSpell(spell4Entry.Spell4BaseIdBaseSpell, (byte)spell4Entry.TierIndex, parameters);
        }

        /// <summary>
        /// Cast a <see cref="ISpell"/> with the supplied spell base id, tier and <see cref="ISpellParameters"/>.
        /// </summary>
        public void CastSpell(uint spell4BaseId, byte tier, ISpellParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException();

            ISpellBaseInfo spellBaseInfo = LegacyServiceProvider.Provider.GetService<ISpellInfoManager>().GetSpellBaseInfo(spell4BaseId);
            if (spellBaseInfo == null)
                throw new ArgumentOutOfRangeException();

            ISpellInfo spellInfo = spellBaseInfo.GetSpellInfo(tier);
            if (spellInfo == null)
                throw new ArgumentOutOfRangeException();

            parameters.SpellInfo = spellInfo;
            CastSpell(parameters);
        }

        /// <summary>
        /// Cast a <see cref="ISpell"/> with the supplied <see cref="ISpellParameters"/>.
        /// </summary>
        public void CastSpell(ISpellParameters parameters)
        {
            if (!IsAlive)
                return;

            if (parameters == null)
                throw new ArgumentNullException();

            if (DisableManager.Instance.IsDisabled(DisableType.BaseSpell, parameters.SpellInfo.BaseInfo.Entry.Id))
            {
                if (this is IPlayer player)
                    player.SendSystemMessage($"Unable to cast base spell {parameters.SpellInfo.BaseInfo.Entry.Id} because it is disabled.");
                return;
            }

            if (DisableManager.Instance.IsDisabled(DisableType.Spell, parameters.SpellInfo.Entry.Id))
            {
                if (this is IPlayer player)
                    player.SendSystemMessage($"Unable to cast spell {parameters.SpellInfo.Entry.Id} because it is disabled.");
                return;
            }

            if (parameters.UserInitiatedSpellCast)
            {
                if (this is IPlayer player)
                    player.Dismount();
            }

            CastMethod castMethod = (CastMethod)parameters.SpellInfo.BaseInfo.Entry.CastMethod;
            if (parameters.ClientSideInteraction != null)
                castMethod = CastMethod.ClientSideInteraction;

            ISpell spell = spellFactory.CreateSpell(castMethod);
            if (spell == null)
                throw new ArgumentNullException();

            spell.Initialise(this, parameters);
            if (!spell.Cast())
                return;

            // Don't store spell if it failed to initialise
            if (spell.IsFailed)
                return;

            pendingSpells.Add(spell);
        }

        /// <summary>
        /// Cancel any <see cref="ISpell"/>'s that are interrupted by movement.
        /// </summary>
        public void CancelSpellsOnMove()
        {
            foreach (ISpell spell in spells.Values)
                if (spell.IsMovingInterrupted() && spell.IsCasting)
                    spell.CancelCast(CastResult.CasterMovement);
        }

        /// <summary>
        /// Finish all active <see cref="ISpell"/>'s that match the supplied predicate.
        /// </summary>
        public void FinishSpells(Func<ISpell, bool> predicate)
        {
            foreach (ISpell spell in spells.Values.Concat(pendingSpells).Where(predicate).ToList())
                spell.Finish();
        }

        /// <summary>
        /// Cancel all <see cref="ISpell"/>'s that are currently casting.
        /// </summary>
        public void CancelSpellCasts()
        {
            foreach (ISpell spell in spells.Values)
                if (spell.IsCasting)
                    spell.CancelCast(CastResult.SpellCancelled);
        }

        /// <summary>
        /// Cancel an <see cref="ISpell"/> based on its casting id.
        /// </summary>
        /// <param name="castingId">Casting ID of the spell to cancel</param>
        public void CancelSpellCast(uint castingId)
        {
            ISpell spell = spells.Values.SingleOrDefault(s => s.CastingId == castingId);
            spell?.CancelCast(CastResult.SpellCancelled);
        }

        /// <summary>
        /// Returns an active <see cref="ISpell"/> that is affecting this <see cref="IUnitEntity"/>
        /// </summary>
        public ISpell GetActiveSpell(Func<ISpell, bool> func)
        {
            return spells.Values.FirstOrDefault(func);
        }

        /// <summary>
        /// Determine if this <see cref="IUnitEntity"/> can attack supplied <see cref="IUnitEntity"/>.
        /// </summary>
        private readonly Dictionary<ISpellTargetEffectInfo, SpellEffectImmunityType> immunities = [];

        /// <summary>
        /// Add a <see cref="SpellEffectImmunityType"/> immunity from the supplied spell effect.
        /// </summary>
        public void AddImmunity(ISpellTargetEffectInfo info, SpellEffectImmunityType type)
        {
            immunities[info] = type;
        }

        /// <summary>
        /// Remove the immunity added by the supplied spell effect.
        /// </summary>
        public void RemoveImmunity(ISpellTargetEffectInfo info)
        {
            immunities.Remove(info);
        }

        /// <summary>
        /// Returns if this <see cref="IUnitEntity"/> is immune to <see cref="SpellEffectImmunityType"/>.
        /// </summary>
        public bool IsImmune(SpellEffectImmunityType type)
        {
            return immunities.ContainsValue(type);
        }

        public virtual bool CanAttack(IUnitEntity target)
        {
            if (!IsAlive)
                return false;

            if (!target.IsValidAttackTarget(this) || !IsValidAttackTarget(target))
                return false;

            return GetDispositionTo(target.Faction1) < Disposition.Friendly;
        }

        /// <summary>
        /// Returns whether or not this <see cref="IUnitEntity"/> is an attackable target for supplied <see cref="IUnitEntity"/>.
        /// </summary>
        public virtual bool IsValidAttackTarget(IUnitEntity attacker)
        {
            // TODO: Expand on this. There's bound to be flags or states that should prevent an entity from being attacked.
            return (this is IPlayer or INonPlayerEntity or ICombatPetEntity);
        }

        /// <summary>
        /// Deal damage to this <see cref="IUnitEntity"/> from the supplied <see cref="IUnitEntity"/>.
        /// </summary>
        public void TakeDamage(IUnitEntity attacker, IDamageDescription damageDescription)
        {
            if (!IsAlive || !attacker.IsAlive || attacker == this)
                return;

            // TODO: Calculate Threat properly
            ThreatManager.UpdateThreat(attacker, (int)damageDescription.RawDamage);

            Shield -= damageDescription.ShieldAbsorbAmount;
            ModifyHealth(damageDescription.AdjustedDamage, damageDescription.DamageType, attacker);
        }

        /// <summary>
        /// Modify the health of this <see cref="IUnitEntity"/> by the supplied amount.
        /// </summary>
        /// <remarks>
        /// If the <see cref="DamageType"/> is <see cref="DamageType.Heal"/> amount is added to current health otherwise subtracted.
        /// </remarks>
        public virtual void ModifyHealth(uint amount, DamageType? type, IUnitEntity source)
        {
            // corpses can't be healed back to full, creatures come back through a respawn and players through resurrection
            if (deathState != null && this is not IPlayer)
                return;

            long newHealth = Health;
            if (type is DamageType.Heal or null)
                newHealth += amount;
            else
                newHealth -= amount;

            Health = (uint)Math.Clamp(newHealth, 0u, MaxHealth);

            scriptCollection?.Invoke<IUnitScript>(s => s.OnHealthChange(source, amount, type));

            if (Health == 0)
                OnDeath(source);
        }

        protected virtual void OnDeath(IUnitEntity killer)
        {
            DeathState = EntityDeathState.JustDied;

            foreach (ISpell spell in spells.Values)
            {
                if (spell.IsCasting)
                    spell.CancelCast(CastResult.CasterCannotBeDead);
            }

            GenerateRewards(killer);
            ProcTrigger.OnKill(killer, this);
            // TODO: schedule respawn

            ThreatManager.ClearThreatList();
            MovementManager.Finalise();

            scriptCollection?.Invoke<IUnitScript>(s => s.OnDeath());

            deathState = EntityDeathState.Dead;
        }

        /// <summary>
        /// Range around the last kill searched for survivors of an exterminate objective's target group.
        /// </summary>
        private const float ExterminateRange = 150f;

        private class SearchCheckAliveTargetGroup : ISearchCheck<IUnitEntity>
        {
            private readonly uint targetGroupId;
            private readonly uint excludeGuid;

            public SearchCheckAliveTargetGroup(uint targetGroupId, uint excludeGuid)
            {
                this.targetGroupId = targetGroupId;
                this.excludeGuid   = excludeGuid;
            }

            public bool CheckEntity(IUnitEntity entity)
            {
                return entity.Guid != excludeGuid
                    && entity.IsAlive
                    && AssetManager.Instance.GetTargetGroupsForCreatureId(entity.CreatureId).Contains(targetGroupId);
            }
        }

        protected virtual void GenerateRewards(IUnitEntity killer)
        {
            foreach (uint targetGroupId in AssetManager.Instance.GetTargetGroupsForCreatureId(CreatureId))
            {
                Map.PublicEventManager.UpdateObjective(PublicEventObjectiveType.KillTargetGroup, targetGroupId, 1);
                // cluster and event objective unit kills name their target group the same way (eg: "Destroy all Blackdeck Cannons!")
                Map.PublicEventManager.UpdateObjective(PublicEventObjectiveType.KillClusterTargetGroup, targetGroupId, 1);
                Map.PublicEventManager.UpdateObjective(PublicEventObjectiveType.KillEventObjectiveUnit, targetGroupId, 1);

                // exterminate objectives ("Wipe out all Ragebots") succeed once none of the target group is left alive around the kill
                if (!Map.Search(Position, ExterminateRange, new SearchCheckAliveTargetGroup(targetGroupId, Guid)).Any())
                    Map.PublicEventManager.UpdateObjective(PublicEventObjectiveType.Exterminate, targetGroupId, 1);
            }

            // kills by combat pets are credited to their owner
            if (killer is ICombatPetEntity pet)
                killer = GetVisible<IUnitEntity>(pet.OwnerGuid) ?? killer;

            // reward public event kill stat to player that landed the killing blow
            if (killer is IPlayer killerPlayer)
                Map.PublicEventManager.UpdateStat(killerPlayer, PublicEventStat.Kills, 1);

            foreach (IHostileEntity hostile in ThreatManager)
            {
                IUnitEntity entity = GetVisible<IUnitEntity>(hostile.HatedUnitId);
                if (entity is IPlayer hostilePlayer)
                    RewardKiller(hostilePlayer);
            }
        }

        protected virtual void RewardKiller(IPlayer player)
        {
            player.QuestManager.ObjectiveUpdate(QuestObjectiveType.KillCreature, CreatureId, 1u);
            player.QuestManager.OnHuntKill(this);
            // counted destructions of a creature ("Call a Greenbough Guardian to destroy Firestorm Flingatrons"), each sets its checklist bit
            player.QuestManager.ObjectiveUpdate(QuestObjectiveType.Unknown10, CreatureId, QuestChecklistMask);
            // "Squash $m(creature=24352)": killing them is the squashing
            player.QuestManager.ObjectiveUpdateUseCreature(CreatureId);

            // "kill creatures of a difficulty or higher", data is the lowest Creature2Difficulty id (inferred: Prime is 6)
            for (uint difficultyId = 1u; difficultyId <= (CreatureInfo?.Entry?.Creature2DifficultyId ?? 0u); difficultyId++)
                player.QuestManager.ObjectiveUpdate(QuestObjectiveType.KillCreature2, difficultyId, 1u);

            foreach (uint targetGroupId in AssetManager.Instance.GetTargetGroupsForCreatureId(CreatureId))
            {
                player.QuestManager.ObjectiveUpdate(QuestObjectiveType.KillTargetGroup, targetGroupId, 1u);
                player.QuestManager.ObjectiveUpdate(QuestObjectiveType.KillTargetGroups, targetGroupId, 1u);
                player.QuestManager.ObjectiveUpdate(QuestObjectiveType.Unknown15, targetGroupId, 1u);
                player.QuestManager.ObjectiveUpdateRewardPane(QuestObjectiveType.KillCreature, targetGroupId, 1u);
            }

            player.QuestManager.ObjectiveUpdateKillInArea(this);

            // quest items dropped by creatures, there is no drop data for these so they drop half the time, a named quest kill always drops its item
            foreach (uint virtualItemId in AssetManager.Instance.GetVirtualCollectItemsForCreatureId(CreatureId))
                if (AssetManager.Instance.IsGuaranteedVirtualCollectItem(CreatureId, virtualItemId) || Random.Shared.Next(2) == 0)
                    player.QuestManager.ObjectiveUpdate(QuestObjectiveType.VirtualCollect, virtualItemId, 1u);

            player.PathMissionManager.OnCreatureKilled(this);
            player.ChallengeManager.OnKill(this);

            scriptCollection?.Invoke<IUnitScript>(s => s.OnRewardKiller(player));

            RewardKillXp(player);
            // TODO: Reward Loot
            // TODO: Handle Achievements
        }

        /// <summary>
        /// Grant experience to the supplied <see cref="IPlayer"/> for killing this unit.
        /// </summary>
        /// <remarks>
        /// The client data has no kill experience values, this is derived from the creature level's base quest experience and its rank:
        /// a Challenger at the player's level gives 12% of a level's base quest experience.
        /// Creatures below the player give 20% less per level and nothing at 5 or more levels below, creatures above give 10% more per level up to 40%.
        /// </remarks>
        private void RewardKillXp(IPlayer player)
        {
            if (this is IPlayer || CreatureInfo?.Entry == null)
                return;

            XpPerLevelEntry levelEntry = GameTableManager.Instance.XpPerLevel.GetEntry(Level);
            Creature2DifficultyEntry difficulty = GameTableManager.Instance.Creature2Difficulty.GetEntry(CreatureInfo.Entry.Creature2DifficultyId);
            if (levelEntry == null || difficulty == null)
                return;

            int levelDifference = (int)Level - (int)player.Level;
            float levelMultiplier = levelDifference >= 0
                ? 1f + 0.1f * Math.Min(levelDifference, 4)
                : Math.Max(0f, 1f + 0.2f * levelDifference);

            // group and raid creatures share their larger contribution across the expected group
            float rankMultiplier = difficulty.ClusterContributionValueDifficulty / Math.Max(1u, difficulty.GroupValue);

            uint xp = (uint)(levelEntry.BaseQuestXpPerLevel * 0.15f * rankMultiplier * levelMultiplier);
            if (xp > 0u)
                player.XpManager.GrantXp(xp, ExpReason.KillCreature);
        }

        /// <summary>
        /// Set target to supplied target guid.
        /// </summary>
        /// <remarks>
        /// A null target will clear the current target.
        /// </remarks>
        public void SetTarget(uint? target, uint threat = 0u)
        {
            SetTarget(target != null ? GetVisible<IWorldEntity>(target.Value) : null, threat);
        }

        /// <summary>
        /// Set target to supplied <see cref="IUnitEntity"/>.
        /// </summary>
        /// <remarks>
        /// A null target will clear the current target.
        /// </remarks>
        public virtual void SetTarget(IWorldEntity target, uint threat = 0u)
        {
            // notify current target they are no longer the target
            if (TargetGuid != null)
                GetVisible<IWorldEntity>(TargetGuid.Value)?.OnUntargeted(this);

            target?.OnTargeted(this);

            EnqueueToVisible(new ServerEntityTargetUnit
            {
                UnitId      = Guid,
                NewTargetId = target?.Guid ?? 0u,
                ThreatLevel = threat
            });

            TargetGuid = target?.Guid;
        }

        /// <summary>
        /// Invoked when a new <see cref="IHostileEntity"/> is added to the threat list.
        /// </summary>
        public virtual void OnThreatAddTarget(IHostileEntity hostile)
        {
            UpdateCombatState();
            scriptCollection?.Invoke<IUnitScript>(s => s.OnThreatAddTarget(hostile));

            // "Taunt" and "draw the attention of" objectives (Support Training: Tanking), target-less activate objectives listing this creature in their reward pane
            if (this is not IPlayer && CreatureId != 0u && GetVisible<IPlayer>(hostile.HatedUnitId) is IPlayer player)
                foreach (uint targetGroupId in AssetManager.Instance.GetTargetGroupsForCreatureId(CreatureId))
                    player.QuestManager.ObjectiveUpdateRewardPane(QuestObjectiveType.ActivateEntity, targetGroupId, 1u);
        }

        /// <summary>
        /// Invoked when an existing <see cref="IHostileEntity"/> is removed from the threat list.
        /// </summary>
        public virtual void OnThreatRemoveTarget(IHostileEntity hostile)
        {
            UpdateCombatState();
            scriptCollection?.Invoke<IUnitScript>(s => s.OnThreatRemoveTarget(hostile));
        }

        /// <summary>
        /// Invoked when an existing <see cref="IHostileEntity"/> is update on the threat list.
        /// </summary>
        public virtual void OnThreatChange(IHostileEntity hostile)
        {
            scriptCollection?.Invoke<IUnitScript>(s => s.OnThreatChange(hostile));
        }

        private void UpdateCombatState()
        {
            // ensure conditions for combat state change are met
            if (ThreatManager.IsThreatened == InCombat)
                return;

            bool previousCombatState = InCombat;
            InCombat = ThreatManager.IsThreatened;

            if (!previousCombatState && InCombat)
            {
                scriptCollection?.Invoke<IUnitScript>(s => s.OnEnterCombat());
                ProcTrigger.OnEnterCombat(this);
            }
            else if (previousCombatState && !InCombat)
                scriptCollection?.Invoke<IUnitScript>(s => s.OnLeaveCombat());

            Sheathed   = !InCombat;
            SetStandState(InCombat ? StandState.Stand : StandState.State0);

            statUpdateManager.OnCombatStateUpdate(InCombat);
        }

        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/> has a <see cref="Stat"/> updated.
        /// </summary>
        protected override void OnStatUpdate(IStatValue statValue, float previousValue)
        {
            statUpdateManager.OnStatUpdate(statValue, previousValue);
        }
    }
}
