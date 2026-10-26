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

    /// <summary>
    /// The Calmwater Ritual (Whitevale): "Remain in the circle until the ritual is complete" once the offerings are made.
    /// </summary>
    /// <remarks>
    /// The Ritual Ring (36763, tagged "Ritual Ring - Q5853") rises in the circle at the objective's marker while the player remains,
    /// for the length of the wait. Its placement in the circle's middle is a guess.
    /// </remarks>
    [ScriptFilterOwnerId(5853)]
    public class CalmwaterRitualQuestScript : QuestSceneScript
    {
        private const uint RemainObjectiveId = 8994u;
        private const uint RitualRing        = 36763u;

        // the objective's marker, WorldLocation2 19605
        private static readonly Vector3 circle = new(2724.8f, -1072.8f, 1190f);

        protected override IReadOnlyList<uint> SceneObjectiveIds => [RemainObjectiveId];

        private readonly ICreatureInfoManager creatureInfoManager;
        private uint? ringGuid;

        public CalmwaterRitualQuestScript(
            ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        protected override void OnSceneStart(uint objectiveId, IPlayer player)
        {
            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(RitualRing);
            if (creatureInfo == null)
                return;

            player.SummonFactory.Summon<ISimpleEntity>(creatureInfo, circle, Vector3.Zero, (map, guid, vector) => ringGuid = guid);
        }

        protected override void OnSceneEnd(uint objectiveId, IPlayer player)
        {
            if (ringGuid != null)
                player.Map?.GetEntity<IWorldEntity>(ringGuid.Value)?.RemoveFromMap();
            ringGuid = null;
        }
    }
}
