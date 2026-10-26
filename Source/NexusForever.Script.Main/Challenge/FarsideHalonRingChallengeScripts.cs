using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Static.Entity.Movement.Spline;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Hot Feet (Farside): "Stamp out the Fire Motes along the Alpha Complex hallway to Bio-Dome 4", each Fire Mote stamped out counts.
    /// Exile (952) and Dominion (954) versions have their own motes.
    /// </summary>
    [ScriptFilterOwnerId(952, 954)]
    public class HotFeetChallengeScript : ChallengeScript
    {
        private static readonly uint[] fireMoteCreatureIds = [45263, 45271];

        public override void OnEntityActivated(IWorldEntity entity)
        {
            if (fireMoteCreatureIds.Contains(entity.CreatureId))
                AddProgress();
        }
    }

    /// <summary>
    /// Races whose finish is somewhere inside a large indicator area, the finish counts as its centre.
    /// </summary>
    public abstract class RaceToAreaChallengeScript : ChallengeScript
    {
        private const float FinishRange = 25f;

        public override void Update(double lastTick)
        {
            WorldLocation2Entry finish = GameTableManager.Instance.WorldLocation2.GetEntry(Challenge.Entry.WorldLocation2IdIndicator);
            if (finish == null || Player.Map?.Entry.Id != finish.WorldId)
                return;

            if (HorizontalDistance(Player.Position, new Vector3(finish.Position0, finish.Position1, finish.Position2)) <= MathF.Min(finish.Radius, FinishRange))
                Complete();
        }
    }

    /// <summary>
    /// Faster and Fearless (Halon Ring): "Complete the Upper Crust speeder race with time to spare!"
    /// </summary>
    [ScriptFilterOwnerId(671)]
    public class FasterAndFearlessChallengeScript : RaceToAreaChallengeScript
    {
    }

    /// <summary>
    /// Dockside Delivery! (Halon Ring): "Race back to the Port of Profit's docks".
    /// </summary>
    [ScriptFilterOwnerId(691)]
    public class DocksideDeliveryChallengeScript : RaceToAreaChallengeScript
    {
    }

    /// <summary>
    /// Full of Life (Farside): "Gather Motes of Primal Life around the Alpha Complex", Exile (948) and Dominion (953) hallways to Bio-Dome 3.
    /// </summary>
    /// <remarks>
    /// Reconstruction, the motes were challenge spawns and Jabbithole saw only two of them. They float at hallway height (the
    /// challenge location's) next to the creatures standing in the hallway, or around the challenge location when there are none.
    /// </remarks>
    [ScriptFilterOwnerId(948, 953)]
    public class FullOfLifeChallengeScript : SummonedTouchChallengeScript
    {
        private const float HallwayHeight = 6f;

        protected override uint CreatureId => 45260u;

        public FullOfLifeChallengeScript(ICreatureInfoManager creatureInfoManager)
            : base(creatureInfoManager)
        {
        }

        protected override List<Vector3> GetPositions()
        {
            WorldLocation2Entry hallway = GameTableManager.Instance.WorldLocation2.GetEntry(Challenge.Entry.WorldLocation2IdIndicator);
            if (hallway == null)
                return [];

            var centre = new Vector3(hallway.Position0, hallway.Position1, hallway.Position2);
            List<Vector3> positions = Player.GetVisible().OfType<IWorldEntity>()
                .Where(e => e is not IPlayer
                    && HorizontalDistance(e.Position, centre) <= hallway.Radius
                    && MathF.Abs(e.Position.Y - centre.Y) <= HallwayHeight)
                .Select(e => e.Position + new Vector3((float)Random.Shared.NextDouble() * 6f - 3f, 1f, (float)Random.Shared.NextDouble() * 6f - 3f))
                .ToList();

            if (positions.Count == 0)
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * MathF.PI / 4f;
                    positions.Add(centre + new Vector3(MathF.Cos(angle) * 10f, 1f, MathF.Sin(angle) * 10f));
                }

            return positions;
        }
    }

    /// <summary>
    /// Spatial Anomalies (Halon Ring): "Bounce on Spatial Anomolies in the Gamma Fields".
    /// </summary>
    /// <remarks>
    /// Reconstruction, the Space Anomalies (65109) came after Jabbithole closed and aren't in any capture. They sit on the ground of
    /// the Gamma Fields within <see cref="FieldRadius"/> of the challenge location and throw the player up when touched. Field size and
    /// bounce height are guesses.
    /// </remarks>
    [ScriptFilterOwnerId(406)]
    public class SpatialAnomaliesChallengeScript : SummonedTouchChallengeScript
    {
        private const float FieldRadius  = 60f;
        private const float BounceHeight = 12f;
        private const float BounceSpeed  = 20f;

        protected override uint CreatureId => 65109u;
        protected override int Count => 8;

        public SpatialAnomaliesChallengeScript(ICreatureInfoManager creatureInfoManager)
            : base(creatureInfoManager)
        {
        }

        protected override List<Vector3> GetPositions()
        {
            WorldLocation2Entry field = GameTableManager.Instance.WorldLocation2.GetEntry(Challenge.Entry.WorldLocation2IdIndicator);
            if (field == null)
                return [];

            var positions = new List<Vector3>();
            for (int i = 0; i < 24; i++)
            {
                float angle = (float)(Random.Shared.NextDouble() * Math.PI * 2d);
                float distance = (float)Random.Shared.NextDouble() * FieldRadius;
                float x = field.Position0 + MathF.Cos(angle) * distance, z = field.Position2 + MathF.Sin(angle) * distance;
                float? y = Player.Map?.GetTerrainHeight(x, z);
                positions.Add(new Vector3(x, y != null && MathF.Abs(y.Value - field.Position1) < 15f ? y.Value : field.Position1, z));
            }
            return positions;
        }

        protected override void OnTouched()
        {
            Vector3 top = Player.Position + new Vector3(0f, BounceHeight, 0f);
            Player.MovementManager.LaunchSpline([Player.Position, top], SplineType.Linear, SplineMode.OneShot, BounceSpeed);
        }
    }
}
