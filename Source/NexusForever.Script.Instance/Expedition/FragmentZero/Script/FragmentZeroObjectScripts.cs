using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;
using NexusForever.Shared.Game;

namespace NexusForever.Script.Instance.Expedition.FragmentZero.Script
{
    /// <summary>
    /// Cargo crates on the surface (Collect cargo crates) and the veteran black box (Locate the ship's Black Box), virtual collect objectives.
    /// </summary>
    [ScriptFilterCreatureId(67965, 69834)]
    public class FragmentZeroCollectableEntityScript : IUnitScript, IOwnedScript<IUnitEntity>
    {
        private IUnitEntity entity;
        private bool collected;

        public void OnLoad(IUnitEntity owner)
        {
            entity = owner;
        }

        public void OnActivateSuccess(IPlayer activator)
        {
            if (collected)
                return;

            collected = true;
            entity.Map.PublicEventManager.GetEvent(680)?.InvokeScriptCollection<FragmentZeroEventScript>(s => s.OnCollect(entity));

            if (entity.InWorld)
                entity.RemoveFromMap();
        }
    }

    /// <summary>
    /// Xenobite eggs in the Incubation Complex (Smash Xenobite Eggs), the "Spawns Xenobite" versions hatch when smashed.
    /// </summary>
    [ScriptFilterCreatureId(68811, 68812, 69132, 69133)]
    public class XenobiteEggEntityScript : IUnitScript, IOwnedScript<IUnitEntity>
    {
        private IUnitEntity entity;

        public void OnLoad(IUnitEntity owner)
        {
            entity = owner;
        }

        public void OnDeath()
        {
            bool hatches = entity.CreatureId is 69132u or 69133u;
            entity.Map.PublicEventManager.GetEvent(680)?.InvokeScriptCollection<FragmentZeroEventScript>(s => s.OnEggSmashed(entity, hatches));
        }
    }

    /// <summary>
    /// Eldan security beams, they burn anyone who walks into them.
    /// </summary>
    [ScriptFilterCreatureId(69317)]
    public class SecurityBeamEntityScript : IUnitScript, IOwnedScript<IUnitEntity>
    {
        private const float Range = 2.5f;

        private IUnitEntity entity;
        private readonly UpdateTimer pulseTimer = new(TimeSpan.FromSeconds(1d));

        #region Dependency Injection

        private readonly IFactory<ISpellParameters> spellParametersFactory;

        public SecurityBeamEntityScript(
            IFactory<ISpellParameters> spellParametersFactory)
        {
            this.spellParametersFactory = spellParametersFactory;
        }

        #endregion

        public void OnLoad(IUnitEntity owner)
        {
            entity = owner;
        }

        public void Update(double lastTick)
        {
            if (entity.Map is not IMapInstance mapInstance)
                return;

            pulseTimer.Update(lastTick);
            if (!pulseTimer.HasElapsed)
                return;
            pulseTimer.Reset();

            foreach (IPlayer player in mapInstance.GetPlayers())
            {
                if (!player.IsAlive || player.Position.GetDistance(entity.Position) > Range)
                    continue;

                ISpellParameters parameters = spellParametersFactory.Resolve();
                parameters.PrimaryTargetId = player.Guid;
                entity.CastSpell(78328, parameters);
            }
        }
    }
}
