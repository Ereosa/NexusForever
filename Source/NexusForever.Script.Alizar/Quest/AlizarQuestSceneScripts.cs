using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Static.Entity.Movement.Spline;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.Quest
{
    /// <summary>
    /// Secure Landing (Galeras): "Wait for Sergeant Kara's arrival in Stormwing Fortress" once the landing flares are placed.
    /// </summary>
    /// <remarks>
    /// Sergeant Kara (17063) and two Skywatch Troopers (17060) land at the drop zone the flares marked and walk to the player's safe spot.
    /// Who lands and the walk are guesses from the objective texts, they leave again a minute later.
    /// </remarks>
    [ScriptFilterOwnerId(4676)]
    public class SecureLandingQuestScript : QuestSceneScript
    {
        private const uint WaitForArrivalObjectiveId = 6459u;
        private const uint SergeantKara              = 17063u;
        private const uint SkywatchTrooper           = 17060u;

        // the first landing flare marker (WorldLocation2 12619) and the safe spot (12642)
        private static readonly Vector3 dropZone = new(6543f, -787.3f, -2646.9f);
        private static readonly Vector3 safeSpot = new(6531.6f, -785.7f, -2631.4f);

        protected override IReadOnlyList<uint> SceneObjectiveIds => [WaitForArrivalObjectiveId];

        #region Dependency Injection

        private readonly ICreatureInfoManager creatureInfoManager;

        public SecureLandingQuestScript(
            ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        protected override void OnSceneStart(uint objectiveId, IPlayer player)
        {
            Arrive(player, SergeantKara, Vector3.Zero);
            Arrive(player, SkywatchTrooper, new Vector3(-2.5f, 0f, 1.5f));
            Arrive(player, SkywatchTrooper, new Vector3(2.5f, 0f, 1.5f));
        }

        private void Arrive(IPlayer player, uint creatureId, Vector3 offset)
        {
            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(creatureId);
            if (creatureInfo == null)
                return;

            Vector3 start = dropZone + offset;
            Vector3 end   = safeSpot + offset + Vector3.Normalize(dropZone - safeSpot) * 4f;
            player.SummonFactory.Summon<INonPlayerEntity>(creatureInfo, start, Vector3.Zero, (map, guid, vector) =>
            {
                IWorldEntity entity = map.GetEntity<IWorldEntity>(guid);
                entity?.MovementManager.LaunchSpline([start, end], SplineType.Linear, SplineMode.OneShot, 4f);
                After(60d, () => map.GetEntity<IWorldEntity>(guid)?.RemoveFromMap());
            });
        }
    }
}
