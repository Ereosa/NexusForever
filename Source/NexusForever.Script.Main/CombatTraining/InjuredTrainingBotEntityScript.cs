using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Static.Spell;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.CombatTraining
{
    /// <summary>
    /// The injured M.A.C.s of Support Training: Healing ("Heal M.A.C. to full health") in the Combat Training Room.
    /// </summary>
    /// <remarks>
    /// Retail scripts damaged them, they stay hurt until healed to full and are hurt again a while later for the next trainee.
    /// The 30% health and 20 second reset are inferred.
    /// </remarks>
    [ScriptFilterCreatureId(70966, 70967)]
    public class InjuredTrainingBotEntityScript : IUnitScript, IOwnedScript<IUnitEntity>
    {
        private const float InjuredHealth = 0.3f;
        private const double ReinjureDelay = 20d;

        private IUnitEntity owner;
        private double reinjureTimer = -1d;

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IUnitEntity owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to <see cref="IBaseMap"/>.
        /// </summary>
        public void OnAddToMap(IBaseMap map)
        {
            Injure();
        }

        /// <summary>
        /// Invoked when health is changed by source <see cref="IUnitEntity"/>.
        /// </summary>
        public void OnHealthChange(IUnitEntity source, uint amount, DamageType? type)
        {
            if (owner.Health >= owner.MaxHealth && reinjureTimer < 0d)
                reinjureTimer = ReinjureDelay;
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            if (reinjureTimer < 0d)
                return;

            reinjureTimer -= lastTick;
            if (reinjureTimer < 0d)
                Injure();
        }

        private void Injure()
        {
            reinjureTimer = -1d;
            uint injured = (uint)(owner.MaxHealth * InjuredHealth);
            if (owner.Health > injured)
                owner.ModifyHealth(owner.Health - injured, DamageType.Physical, null);
        }
    }
}
