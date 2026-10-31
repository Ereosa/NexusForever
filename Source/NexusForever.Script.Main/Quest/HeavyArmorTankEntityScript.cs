using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Spell;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Quest
{
    /// <summary>
    /// Heavy Armor (Crimson Isle, 5583): "Sabotage the Exile Tanks defending Megatech Station", a kill objective on the Hellfire and
    /// Vindicator Tanks (target group 3737).
    /// </summary>
    /// <remarks>
    /// The tanks' activate spell (35719) sets them busy and deals its damage 3 seconds later, the sabotage destroys the tank (report 134).
    /// The tank wasn't fighting the player so the kill was never credited; a tank still standing after the sabotage is destroyed by the
    /// saboteur, the kill counts for them.
    /// </remarks>
    [ScriptFilterCreatureId(24255, 24452)]
    public class HeavyArmorTankEntityScript : IUnitScript, IOwnedScript<IUnitEntity>
    {
        private const double SabotageDelay = 3.5d;

        private IUnitEntity owner;
        private uint saboteurGuid;
        private double timer = -1d;

        public void OnLoad(IUnitEntity owner)
        {
            this.owner = owner;
        }

        public void OnActivateSuccess(IPlayer activator)
        {
            if (timer >= 0d)
                return;

            saboteurGuid = activator.Guid;
            timer = SabotageDelay;
        }

        public void Update(double lastTick)
        {
            if (timer < 0d)
                return;

            timer -= lastTick;
            if (timer > 0d)
                return;

            timer = -1d;
            if (!owner.IsAlive || owner.Map?.GetEntity<IPlayer>(saboteurGuid) is not IPlayer saboteur)
                return;

            owner.ModifyHealth(owner.MaxHealth, DamageType.Physical, saboteur);
        }
    }
}
