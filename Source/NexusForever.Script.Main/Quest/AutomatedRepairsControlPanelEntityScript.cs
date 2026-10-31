using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Quest
{
    /// <summary>
    /// Eldan Control Panels in Security Hub Sigma, Levian Bay (Automated Repairs): each one releases a Repair Construct.
    /// </summary>
    /// <remarks>
    /// The objectives ask to activate the panel "to release a Repair Construct", nothing in the game tables spawns them (report 131).
    /// The construct appears beside the panel for a while, its position and how long it stays are guessed.
    /// </remarks>
    [ScriptFilterCreatureId(26173, 51051, 51052, 51053)]
    public class AutomatedRepairsControlPanelEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint RepairConstructCreatureId = 26166;
        private static readonly TimeSpan ConstructDuration = TimeSpan.FromMinutes(2d);

        #region Dependency Injection

        private readonly ICreatureInfoManager creatureInfoManager;

        public AutomatedRepairsControlPanelEntityScript(
            ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        private IWorldEntity owner;

        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/> is successfully activated by <see cref="IPlayer"/>.
        /// </summary>
        public void OnActivateSuccess(IPlayer activator)
        {
            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(RepairConstructCreatureId);
            if (creatureInfo == null || owner.Map == null)
                return;

            // the construct steps out in front of the panel
            float angle = owner.Rotation.X;
            Vector3 position = owner.Position + new Vector3(-MathF.Sin(angle), 0f, -MathF.Cos(angle)) * 3f;
            owner.SummonFactory.Summon<INonPlayerEntity>(creatureInfo, position, owner.Rotation,
                (map, guid, vector) => map.ScheduleDespawn(guid, ConstructDuration));
        }
    }
}
