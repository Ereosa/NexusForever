using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Treeclimber (Deradune): "Reach the top of the tree before time runs out", from the base of the canopy tree (start location 33222).
    /// </summary>
    /// <remarks>
    /// The tree is map geometry and its height isn't in any data, the top counts as anywhere in the tree's area (indicator 21049)
    /// at least <see cref="ClimbHeight"/> above the start.
    /// </remarks>
    [ScriptFilterOwnerId(22)]
    public class TreeclimberChallengeScript : ChallengeScript
    {
        private const float ClimbHeight = 20f;

        public override void Update(double lastTick)
        {
            WorldLocation2Entry start = GameTableManager.Instance.WorldLocation2.GetEntry(Challenge.Entry.WorldLocation2IdStartLocation);
            WorldLocation2Entry tree  = GameTableManager.Instance.WorldLocation2.GetEntry(Challenge.Entry.WorldLocation2IdIndicator);
            if (start == null || tree == null)
                return;

            if (HorizontalDistance(Player.Position, new Vector3(tree.Position0, tree.Position1, tree.Position2)) <= tree.Radius
                && Player.Position.Y >= start.Position1 + ClimbHeight)
                Complete();
        }
    }

    /// <summary>
    /// Owanee Speedway (Deradune): "Race from Grimvoid Landing to Owanee Research Station on a Grimvoid Speeder before time runs out".
    /// </summary>
    [ScriptFilterOwnerId(325)]
    public class OwaneeSpeedwayChallengeScript : ChallengeScript
    {
        private const uint OwaneeResearchStationZoneId = 639;

        public override void Update(double lastTick)
        {
            for (WorldZoneEntry zone = Player.Zone; zone != null; zone = zone.ParentZoneId != 0u ? GameTableManager.Instance.WorldZone.GetEntry(zone.ParentZoneId) : null)
            {
                if (zone.Id != OwaneeResearchStationZoneId)
                    continue;

                Complete();
                return;
            }
        }
    }

    /// <summary>
    /// What's on Tap? (Ellevar): "Serve the patrons their drinks as quickly as possible" in the Emperor's Crown Tavern, pouring at the
    /// Bartender's Bottle (C460) and carrying each drink to a patron.
    /// </summary>
    /// <remarks>
    /// The C460 patrons (31485 to 31487) aren't in the world spawns, they are seated on the tavern stools nearest the bottle while the
    /// challenge runs. Using the bottle starts the challenge with the first drink poured.
    /// </remarks>
    [ScriptFilterOwnerId(460)]
    public class WhatsOnTapChallengeScript : ChallengeScript
    {
        private const uint BottleCreatureId = 31482;
        private const uint StoolCreatureId  = 57205;
        private const float ServeRange      = 2.5f;
        private const float TavernRange     = 15f;

        private static readonly uint[] patronCreatureIds = [31485, 31486, 31487];

        private readonly ICreatureInfoManager creatureInfoManager;
        private readonly List<uint> patrons = [];
        private bool holdingDrink;

        public WhatsOnTapChallengeScript(ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        public override void OnActivate()
        {
            holdingDrink = true;
            patrons.Clear();

            IWorldEntity bottle = Player.GetVisibleCreature<IWorldEntity>(BottleCreatureId).FirstOrDefault();
            if (bottle == null)
                return;

            List<IWorldEntity> stools = Player.GetVisibleCreature<IWorldEntity>(StoolCreatureId)
                .Where(s => Vector3.Distance(s.Position, bottle.Position) <= TavernRange)
                .OrderBy(s => Vector3.Distance(s.Position, bottle.Position))
                .Take(patronCreatureIds.Length)
                .ToList();

            for (int i = 0; i < stools.Count; i++)
            {
                ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(patronCreatureIds[i]);
                if (creatureInfo != null)
                    Player.SummonFactory.Summon(creatureInfo, stools[i].Position, stools[i].Rotation, (map, guid, position) => patrons.Add(guid));
            }
        }

        public override void OnEntityActivated(IWorldEntity entity)
        {
            if (entity.CreatureId == BottleCreatureId)
                holdingDrink = true;
        }

        public override void Update(double lastTick)
        {
            if (!holdingDrink)
                return;

            foreach (uint guid in patrons)
            {
                IWorldEntity patron = Player.GetVisible<IWorldEntity>(guid);
                if (patron == null || Vector3.Distance(patron.Position, Player.Position) > ServeRange)
                    continue;

                holdingDrink = false;
                AddProgress();
                return;
            }
        }

        public override void OnStop(bool completed)
        {
            foreach (uint guid in patrons)
                Player.SummonFactory.Unsummon(guid);
            patrons.Clear();
        }
    }

    /// <summary>
    /// Lowland Lavender Eggs (Ellevar): "Collect Lowland Lavender Eggs in the Larallen Lowlands", each Lowland Lavender Eggy used counts.
    /// </summary>
    [ScriptFilterOwnerId(505)]
    public class LowlandLavenderEggsChallengeScript : ChallengeScript
    {
        private const uint EggCreatureId = 33152;

        public override void OnEntityActivated(IWorldEntity entity)
        {
            if (entity.CreatureId == EggCreatureId)
                AddProgress();
        }
    }

    /// <summary>
    /// Larallen Lake Currents (Ellevar): "Swim through Larallen lake currents in Lake Larallen", each Larallen Lake Current counts once.
    /// </summary>
    [ScriptFilterOwnerId(494)]
    public class LarallenLakeCurrentsChallengeScript : TouchChallengeScript
    {
        protected override uint[] CreatureIds { get; } = [32094];
        protected override float TouchRange => 4f;
    }

    /// <summary>
    /// Can You Dig It? (Ellevar): "Find the location of the buried treasure on the Southtech Ledges" by digging the Ch508 rock mounds.
    /// </summary>
    /// <remarks>
    /// The goal mound (33474) isn't in the world spawns. The treasure is buried under one of the clue mounds picked when the challenge
    /// starts, digging any other mound raises a Rock Mound Arrow pointing towards it for a few seconds.
    /// </remarks>
    [ScriptFilterOwnerId(508)]
    public class CanYouDigItChallengeScript : ChallengeScript
    {
        private const uint ClueMoundCreatureId = 33475;
        private const uint ArrowCreatureId     = 33476;
        private const double ArrowDuration     = 8d;

        private readonly ICreatureInfoManager creatureInfoManager;
        private readonly List<(uint Guid, double Expiry)> arrows = [];
        private uint treasureGuid;
        private double time;

        public CanYouDigItChallengeScript(ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        public override void OnActivate()
        {
            time         = 0d;
            treasureGuid = 0u;

            List<IWorldEntity> mounds = Player.GetVisibleCreature<IWorldEntity>(ClueMoundCreatureId).ToList();
            if (mounds.Count > 0)
                treasureGuid = mounds[Random.Shared.Next(mounds.Count)].Guid;
        }

        public override void OnEntityActivated(IWorldEntity entity)
        {
            if (entity.CreatureId != ClueMoundCreatureId)
                return;

            if (treasureGuid == 0u)
                treasureGuid = entity.Guid;

            if (entity.Guid == treasureGuid)
            {
                Complete();
                return;
            }

            IWorldEntity treasure = Player.GetVisible<IWorldEntity>(treasureGuid);
            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(ArrowCreatureId);
            if (treasure == null || creatureInfo == null)
                return;

            // face the arrow towards the treasure, entity rotations are yaw first
            Vector3 direction = treasure.Position - entity.Position;
            float yaw = MathF.Atan2(-direction.X, -direction.Z);
            Player.SummonFactory.Summon(creatureInfo, entity.Position, new Vector3(yaw, 0f, 0f), (map, guid, position) => arrows.Add((guid, time + ArrowDuration)));
        }

        public override void Update(double lastTick)
        {
            time += lastTick;
            foreach ((uint guid, double expiry) in arrows.Where(a => time >= a.Expiry).ToList())
            {
                Player.SummonFactory.Unsummon(guid);
                arrows.Remove((guid, expiry));
            }
        }

        public override void OnStop(bool completed)
        {
            foreach ((uint guid, double _) in arrows)
                Player.SummonFactory.Unsummon(guid);
            arrows.Clear();
        }
    }

    /// <summary>
    /// Shock to Your Systems (Ellevar): "Disarm Eldan Security Mines without getting shocked" on Recombination Complex Level 2.
    /// </summary>
    /// <remarks>
    /// The goal is every Eldan Security Mine (33548) around the challenge indicator when it starts, tiers are the time left. Getting
    /// shocked isn't told apart from other damage, taking any damage fails the run.
    /// </remarks>
    [ScriptFilterOwnerId(515)]
    public class ShockToYourSystemsChallengeScript : ChallengeScript
    {
        private const uint MineCreatureId = 33548;

        private uint goal;
        private uint disarmed;
        private uint health;

        public override void OnActivate()
        {
            disarmed = 0u;
            health   = Player.Health;

            WorldLocation2Entry area = GameTableManager.Instance.WorldLocation2.GetEntry(Challenge.Entry.WorldLocation2IdIndicator);
            goal = area == null ? 0u : (uint)Player.GetVisibleCreature<IWorldEntity>(MineCreatureId)
                .Count(m => HorizontalDistance(m.Position, new Vector3(area.Position0, area.Position1, area.Position2)) <= area.Radius);
        }

        public override void OnEntityActivated(IWorldEntity entity)
        {
            if (entity.CreatureId != MineCreatureId)
                return;

            disarmed++;
            AddProgress();
            if (Challenge.Activated && goal > 0u && disarmed >= goal)
                Complete();
        }

        public override void Update(double lastTick)
        {
            uint current = Player.Health;
            if (current < health)
            {
                Fail();
                return;
            }

            health = current;
        }
    }
}
