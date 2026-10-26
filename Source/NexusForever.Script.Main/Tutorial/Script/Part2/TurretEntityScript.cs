using System.Diagnostics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Reputation;
using NexusForever.Game.Static.Spell;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;
using NexusForever.Shared.Game;

namespace NexusForever.Script.Main.Tutorial.Script.Part2
{
    /// <summary>
    /// Dominion turrets (Exile simulation) and Exile turrets (Dominion simulation) in The Face of the Enemy.
    /// </summary>
    /// <remarks>
    /// The turret is immune to damage and charges an 8 second shot at the player. Interrupting the charge gives a
    /// 15 second Moment of Opportunity where the immunity is removed and the turret can be destroyed.
    /// </remarks>
    [ScriptFilterCreatureId(DominionTurret, ExileTurret)]
    public class TurretEntityScript : IUnitScript, IOwnedScript<ICreatureEntity>
    {
        private const uint DominionTurret = 73494;
        private const uint ExileTurret    = 74862;

        private enum SpellId
        {
            Charging            = 85469,
            ChargingExile       = 86978,
            MomentOfOpportunity = 85472,
            TurretImmune        = 85474
        }

        private const float Range = 25f;
        private static readonly TimeSpan ChargeTime          = TimeSpan.FromSeconds(8d);
        private static readonly TimeSpan OpportunityDuration = TimeSpan.FromSeconds(15d);
        private static readonly TimeSpan ChargeCooldown      = TimeSpan.FromSeconds(2d);

        private ICreatureEntity owner;

        private ISpell charge;
        private readonly Stopwatch chargeTimer = new();
        private readonly UpdateTimer cooldown = new(ChargeCooldown, false);
        private readonly UpdateTimer opportunity = new(OpportunityDuration, false);
        private bool opportunityPending;

        #region Dependency Injection

        private readonly IFactory<ISpellParameters> spellParameterFactory;

        public TurretEntityScript(
            IFactory<ISpellParameters> spellParameterFactory)
        {
            this.spellParameterFactory = spellParameterFactory;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(ICreatureEntity owner)
        {
            this.owner = owner;
            TutorialFaction.MakeHostile(owner, owner.CreatureId == ExileTurret ? Faction.Dominion : Faction.Exile);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to <see cref="IBaseMap"/>.
        /// </summary>
        public void OnAddToMap(IBaseMap map)
        {
            Cast(SpellId.TurretImmune, owner.Guid);
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            if (!owner.IsAlive)
                return;

            if (opportunity.IsTicking)
            {
                // the interrupt leaves the turret crowd controlled for a few seconds and Moment of Opportunity can't be cast while it is
                if (opportunityPending)
                    opportunityPending = !CastOpportunity();

                opportunity.Update(lastTick);
                if (opportunity.HasElapsed)
                {
                    opportunity.Reset(false);
                    Cast(SpellId.TurretImmune, owner.Guid);
                }
                return;
            }

            if (charge != null)
            {
                if (!charge.IsFinished)
                    return;

                // a charge that ended early was interrupted
                if (chargeTimer.Elapsed < ChargeTime - TimeSpan.FromSeconds(0.5d))
                {
                    // the immunity drops straight away, the Moment of Opportunity visual follows once the interrupt wears off
                    RemoveImmunity();
                    opportunityPending = !CastOpportunity();
                    opportunity.Reset(true);
                }

                charge = null;
                cooldown.Reset(true);
                return;
            }

            if (cooldown.IsTicking)
            {
                cooldown.Update(lastTick);
                if (!cooldown.HasElapsed)
                    return;
                cooldown.Reset(false);
            }

            IPlayer target = owner.Map.Search(owner.Position, Range, new SearchCheckHostilePlayer(owner)).FirstOrDefault();
            if (target == null)
                return;

            owner.MovementManager.SetRotationFaceUnit(target.Guid);
            SpellId charging = owner.CreatureId == ExileTurret ? SpellId.ChargingExile : SpellId.Charging;
            Cast(charging, target.Guid);
            if (owner.HasSpell(s => s.Spell4Id == (uint)charging && s.IsCasting, out ISpell spell))
            {
                charge = spell;
                chargeTimer.Restart();
            }
        }

        /// <summary>
        /// Invoked when <see cref="IUnitEntity"/> is killed.
        /// </summary>
        public void OnDeath()
        {
            charge = null;
            opportunityPending = false;
            opportunity.Reset(false);
        }

        private bool CastOpportunity()
        {
            Cast(SpellId.MomentOfOpportunity, owner.Guid);
            return owner.HasSpell(s => s.Spell4Id == (uint)SpellId.MomentOfOpportunity && !s.IsFinished, out ISpell _);
        }

        private void RemoveImmunity()
        {
            foreach (ISpell spell in owner.GetSpellsByEffect(SpellEffectType.SpellEffectImmunity)
                .Where(s => !s.IsFinished && s.Spell4Id == (uint)SpellId.TurretImmune)
                .ToList())
                spell.Finish();
        }

        private void Cast(SpellId spell, uint targetId)
        {
            ISpellParameters parameters = spellParameterFactory.Resolve();
            parameters.PrimaryTargetId        = targetId;
            parameters.UserInitiatedSpellCast = false;
            owner.CastSpell((uint)spell, parameters);
        }

        internal class SearchCheckHostilePlayer : Game.Abstract.Map.Search.ISearchCheck<IPlayer>
        {
            private readonly ICreatureEntity owner;

            public SearchCheckHostilePlayer(ICreatureEntity owner)
            {
                this.owner = owner;
            }

            public bool CheckEntity(IPlayer entity)
            {
                return entity.IsAlive && owner.GetDispositionTo(entity.Faction1) == Disposition.Hostile;
            }
        }
    }
}
