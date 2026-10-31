using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Seek and Destroy: "Destroy alchemical supply bins throughout Grimbone Tomb", each Alchemical Supplies object used counts.
    /// </summary>
    [ScriptFilterOwnerId(620)]
    public class SeekAndDestroyChallengeScript : ChallengeScript
    {
        private const uint AlchemicalSuppliesCreatureId = 37391;

        public override void OnEntityActivated(IWorldEntity entity)
        {
            if (entity.CreatureId == AlchemicalSuppliesCreatureId)
                AddProgress();
        }
    }

    /// <summary>
    /// Race to the XAS Dig Site: "Reach Stonemore's plateau before time runs out", the plateau is the challenge indicator around Stonemore.
    /// </summary>
    [ScriptFilterOwnerId(194)]
    public class RaceToTheXasDigSiteChallengeScript : ChallengeScript
    {
        /// <summary>
        /// The plateau is level with the indicator, the dig site terminals below it are 55 units lower.
        /// </summary>
        private const float PlateauHeightTolerance = 6f;

        public override void Update(double lastTick)
        {
            if (IsAtOrAbove(Challenge.Entry.WorldLocation2IdIndicator, PlateauHeightTolerance))
                Complete();
        }
    }

    /// <summary>
    /// Bull's-Eye!: "Land in the center of the pond after being fired out of the XAS retrofitted cannon", scored 1 to 100 by how close
    /// to the centre of the pond (the challenge indicator) the player lands, bronze 25, silver 50 and gold 100.
    /// </summary>
    /// <remarks>
    /// The cannon (creature 37346, spell 40526) launches the player with ForcedMove effects. A landing only counts after being launched,
    /// once the player has been well above the cannon, and the first landing in the pond ends the run.
    /// </remarks>
    [ScriptFilterOwnerId(621)]
    public class BullsEyeChallengeScript : ChallengeScript
    {
        private const float LaunchHeight      = 10f;
        private const float SplashTolerance   = 3f;

        private WorldLocation2Entry cannon;
        private WorldLocation2Entry pond;
        private bool launched;

        public override void OnActivate()
        {
            cannon   = GameTableManager.Instance.WorldLocation2.GetEntry(Challenge.Entry.WorldLocation2IdStartLocation);
            pond     = GameTableManager.Instance.WorldLocation2.GetEntry(Challenge.Entry.WorldLocation2IdIndicator);
            launched = false;
        }

        public override void Update(double lastTick)
        {
            if (cannon == null || pond == null)
                return;

            if (!launched)
            {
                launched = Player.Position.Y >= cannon.Position1 + LaunchHeight;
                return;
            }

            if (Math.Abs(Player.Position.Y - pond.Position1) > SplashTolerance)
                return;

            float distance = HorizontalDistance(Player.Position, new Vector3(pond.Position0, pond.Position1, pond.Position2));
            if (distance > pond.Radius)
            {
                // missed the pond, landing anywhere else ends the shot
                if (Player.Position.Y <= pond.Position1 + SplashTolerance)
                    Fail();
                return;
            }

            uint score = (uint)Math.Clamp(Math.Ceiling(100d * (1d - distance / pond.Radius)), 1d, 100d);
            AddProgress(score);

            if (!Challenge.Activated)
                return;

            if (Challenge.CurrentTier > 0u)
                Complete();
            else
                Fail();
        }
    }

    /// <summary>
    /// The Grid: "Navigate the Chua security lasers without touching any beams" through the Chua Collective beam emitters (creature 15187).
    /// </summary>
    /// <remarks>
    /// The beams themselves are client visuals between the emitters and their layout isn't in any data, touching an emitter stands in for
    /// touching a beam. The course runs across the grid along X, the run is finished on reaching the far side from where it started.
    /// Touching a beam sends the player back to where they entered the grid (report 99), the touch range is guessed.
    /// </remarks>
    [ScriptFilterOwnerId(116)]
    public class TheGridChallengeScript : ChallengeScript
    {
        private const uint BeamEmitterCreatureId = 15187;
        private const float TouchRange  = 1.25f;
        private const float TouchHeight = 3f;

        /// <summary>
        /// Extent of the emitters spawned in Algoroc.
        /// </summary>
        private const float GridWestX  = 3584f;
        private const float GridEastX  = 3619f;
        private const float GridNorthZ = -3600f;
        private const float GridSouthZ = -3560f;

        private bool? startedWest;
        private Vector3 lastOutside;
        private Vector3 start;

        public override void OnActivate()
        {
            startedWest = null;
            lastOutside = Player.Position;
        }

        public override void Update(double lastTick)
        {
            Vector3 position = Player.Position;
            bool inLanes = position.Z >= GridNorthZ && position.Z <= GridSouthZ;

            if (startedWest == null)
            {
                // the run starts when the player steps into the grid from one of its ends
                if (inLanes && position.X >= GridWestX && position.X <= GridEastX)
                {
                    startedWest = position.X - GridWestX < GridEastX - position.X;
                    start = lastOutside;
                }
                else
                    lastOutside = position;
                return;
            }

            foreach (IWorldEntity emitter in Player.GetVisibleCreature<IWorldEntity>(BeamEmitterCreatureId))
            {
                if (HorizontalDistance(emitter.Position, position) <= TouchRange && Math.Abs(emitter.Position.Y - position.Y) <= TouchHeight)
                {
                    IPlayer player = Player;
                    Fail();
                    if (player.CanTeleport())
                        player.TeleportToLocal(start);
                    return;
                }
            }

            if (inLanes && (startedWest.Value ? position.X > GridEastX : position.X < GridWestX))
                Complete();
        }
    }
}

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Scrap Yard: "Collect Eldan Scrap near the Excavation Site before time runs out", the challenge names the Eldan Scrap virtual
    /// item but no creature gives it in the data, it comes from the CH80 Eldan Scrap objects.
    /// </summary>
    [ScriptFilterOwnerId(80)]
    public class ScrapYardChallengeScript : ChallengeScript
    {
        private const uint EldanScrapCreatureId = 18441;

        public override void OnEntityActivated(NexusForever.Game.Abstract.Entity.IWorldEntity entity)
        {
            if (entity.CreatureId == EldanScrapCreatureId)
                AddProgress();
        }
    }
}
