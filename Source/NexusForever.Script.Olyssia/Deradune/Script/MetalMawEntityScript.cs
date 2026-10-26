using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Olyssia.Deradune.Event;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Olyssia.Deradune.Script
{
    /// <summary>
    /// Metal Maw, Deradune world boss, casts the "[WB] Deradune - Metal Maw" spells on his target while the generic combat AI fights.
    /// </summary>
    /// <remarks>
    /// The ability list is the Deradune world boss spells of the client, cooldowns and order are guessed.
    /// </remarks>
    [ScriptFilterCreatureId(24740)]
    public class MetalMawEntityScript : IUnitScript, IOwnedScript<ICreatureEntity>
    {
        private class Ability
        {
            public uint SpellId { get; init; }
            public double Cooldown { get; init; }
            public double InitialDelay { get; init; }
            public double Timer { get; set; }
        }

        private readonly List<Ability> abilities =
        [
            new() { SpellId = 80143, Cooldown = 14d, InitialDelay = 6d  }, // Foot Stomp (right)
            new() { SpellId = 80144, Cooldown = 14d, InitialDelay = 13d }, // Foot Stomp (left)
            new() { SpellId = 63363, Cooldown = 18d, InitialDelay = 9d  }, // Caustic Belch
            new() { SpellId = 80146, Cooldown = 25d, InitialDelay = 16d }, // Drone Missile
            new() { SpellId = 40797, Cooldown = 30d, InitialDelay = 22d }, // Thrust Blitz
            new() { SpellId = 41304, Cooldown = 40d, InitialDelay = 30d }, // Plasma Beam
            new() { SpellId = 40907, Cooldown = 45d, InitialDelay = 40d }, // Reckless Bombardment
            new() { SpellId = 63374, Cooldown = 60d, InitialDelay = 50d }  // Release Drones
        ];

        private ICreatureEntity entity;

        #region Dependency Injection

        private readonly IFactory<ISpellParameters> spellParametersFactory;
        private readonly IGameTableManager gameTableManager;

        public MetalMawEntityScript(
            IFactory<ISpellParameters> spellParametersFactory,
            IGameTableManager gameTableManager)
        {
            this.spellParametersFactory = spellParametersFactory;
            this.gameTableManager       = gameTableManager;
        }

        #endregion

        public void OnLoad(ICreatureEntity owner)
        {
            entity = owner;
        }

        public void OnEnterCombat()
        {
            foreach (Ability ability in abilities)
                ability.Timer = ability.InitialDelay;
        }

        public void OnDeath()
        {
            // the kill objective is a target group kill, in case the killing blow didn't count for the event
            entity.Map?.PublicEventManager.GetEvent(MetalMawEventScript.EventId)?
                .UpdateObjective((uint)MetalMawEventScript.Objective.DefeatMetalMaw, 1);
        }

        public void Update(double lastTick)
        {
            if (!entity.IsAlive || !entity.InCombat || entity.TargetGuid == null)
                return;

            foreach (Ability ability in abilities)
                ability.Timer -= lastTick;

            if (entity.IsCasting())
                return;

            IUnitEntity target = entity.GetVisible<IUnitEntity>(entity.TargetGuid.Value);
            if (target == null || !target.IsAlive)
                return;

            foreach (Ability ability in abilities)
            {
                if (ability.Timer > 0d)
                    continue;

                Spell4Entry entry = gameTableManager.Spell4.GetEntry(ability.SpellId);
                if (entry == null)
                    continue;

                if (entry.TargetMaxRange > 0f && Vector3.Distance(entity.Position, target.Position) > entry.TargetMaxRange)
                    continue;

                ISpellParameters parameters = spellParametersFactory.Resolve();
                parameters.PrimaryTargetId = target.Guid;
                entity.CastSpell(ability.SpellId, parameters);
                ability.Timer = ability.Cooldown;
                break;
            }
        }
    }
}
