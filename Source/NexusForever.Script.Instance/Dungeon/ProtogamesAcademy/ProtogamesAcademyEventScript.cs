using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Dungeon.ProtogamesAcademy
{
    /// <summary>
    /// Protogames Academy (public event 667, world 3173): Phineas A. Rotostar's training dungeon, seven assessments in three rooms.
    /// </summary>
    /// <remarks>
    /// No packet captures and no written guide exist, the order follows the objective data. Each "Gather" objective is a trigger volume at
    /// the arena of the next boss (its WorldLocation2), the boss objectives name their arena the same way:
    /// - the lobby (zone 4509): click the Start Button (objective 4341),
    /// - room 1, the Basic Confrontation Zone (zones 4651 and 4652): gather (4490) and defeat Invulnotron (4342), gather in the second arena
    ///   (4493) and defeat Gromka the Flamewitch (4496), gather in the first arena again (4492) and defeat Iruki Boldbeard (4495), take
    ///   the teleporter (turnstile 4501),
    /// - room 2 (zone 4507): gather (4491) and defeat Seek-N-Slaughter (4497), gather (4494) and defeat Icebox Mk. 2 (4498), gather (4343)
    ///   and defeat Super-Invulnotron (4499), take the teleporter (turnstile 4500),
    /// - room 3 (zone 4508): meet Phineas (4345) and defeat Wrathbone (4346).
    /// The bosses appear in their arena once the whole party has gathered, veteran (prime) runs get the veteran creatures.
    /// Guessed: that every room 1 and room 2 boss is fought (rather than a random pick), the order within a room, that the Start Button
    /// carries the party to room 1 and the teleporters to the next room's gather spot, where Phineas and the Start Button stand.
    /// </remarks>
    [ScriptFilterOwnerId(667)]
    public class ProtogamesAcademyEventScript : ExpeditionEventScript
    {
        private enum Phase : uint
        {
            Start,
            GatherInvulnotron,
            Invulnotron,
            GatherGromka,
            Gromka,
            GatherIruki,
            Iruki,
            TeleporterRoom2,
            GatherSeek,
            Seek,
            GatherIcebox,
            Icebox,
            GatherSuperInvulnotron,
            SuperInvulnotron,
            TeleporterRoom3,
            MeetPhineas,
            Wrathbone,
            Done
        }

        private static class Objective
        {
            public const uint Start            = 4341;
            public const uint Invulnotron      = 4342;
            public const uint GatherRoom2Third = 4343;
            public const uint MeetPhineas      = 4345;
            public const uint Wrathbone        = 4346;
            public const uint GatherFirst      = 4490;
            public const uint GatherRoom2First = 4491;
            public const uint GatherRoom1Third = 4492;
            public const uint GatherRoom1Second = 4493;
            public const uint GatherRoom2Second = 4494;
            public const uint Iruki            = 4495;
            public const uint Gromka           = 4496;
            public const uint Seek             = 4497;
            public const uint Icebox           = 4498;
            public const uint SuperInvulnotron = 4499;
            public const uint TeleporterRoom3  = 4500;
            public const uint TeleporterRoom2  = 4501;
            public const uint Deathless        = 4436;
        }

        private static class TriggerVolume
        {
            public const uint Room1Arena1 = 7932;
            public const uint Room1Arena2 = 7933;
            public const uint Room2       = 7934;
            public const uint Room3       = 7940;
        }

        private static readonly Dictionary<uint, uint> Veteran = new()
        {
            { 67475, 71082 }, { 67594, 71319 }, { 67609, 71320 }, { 67619, 71321 }, { 67663, 71538 }, { 67690, 71581 },
            { 67668, 71203 }, { 67724, 71204 }, { 67757, 71205 }, { 67738, 71207 }, { 68242, 71208 }, { 68096, 71209 },
            { 68101, 71213 }, { 67944, 71226 }, { 67977, 71229 }
        };

        public const uint Invulnotron      = 67475;
        public const uint Gromka           = 67594;
        public const uint Iruki            = 67663;
        public const uint Seek             = 67668;
        public const uint Icebox           = 67757;
        public const uint SuperInvulnotron = 68096;
        public const uint Wrathbone        = 67944;
        public const uint StartButton      = 68339;
        private const uint Phineas         = 68342;
        private const uint PhineasRoom3    = 67504;
        private const uint ExitPortal      = 69790;
        private const uint Room3Gate       = 68396;

        private static readonly Vector3 Lobby             = new(-29439.2f, -775.2f, -29386.4f); // WorldLocation2 48284, the map entrance
        private static readonly Vector3 Room1Arena1       = new(-24400.4f, -974.6f, -28977.2f); // 48842, objectives 4490, 4492, 4501
        private static readonly Vector3 Room1Arena2       = new(-24504.8f, -974.6f, -28857.0f); // 48843, objective 4493
        private static readonly Vector3 Room2             = new(-19803.9f, -945.5f, -29483.6f); // 48846, objectives 4491, 4494, 4343, 4500
        private static readonly Vector3 Room3             = new(-15760.8f, -904.5f, -29638.8f); // 48850, objective 4345
        private static readonly Vector3 InvulnotronSpot   = new(-24330.6f, -974.2f, -28917.8f); // 50173, objective 4342
        private static readonly Vector3 IrukiSpot         = new(-24360.6f, -972.9f, -28943.5f); // 48346, objective 4495
        private static readonly Vector3 GromkaSpot        = new(-24424.5f, -974.0f, -28788.2f); // 48386, objective 4496
        private static readonly Vector3 Room2Arena        = new(-19777.3f, -946.4f, -29457.3f); // 50174, objectives 4497, 4498, 4499
        private static readonly Vector3 WrathboneSpot     = new(-15808.7f, -900.1f, -29596.5f); // 50175, objective 4346

        protected override uint WorldId => 3173;
        protected override uint InitialPhase => (uint)Phase.Start;
        protected override IReadOnlyDictionary<uint, uint> VeteranCreatures => Veteran;

        private Phase teleportPending = Phase.Done;

        public ProtogamesAcademyEventScript(
            IGlobalQuestManager globalQuestManager,
            ICreatureInfoManager creatureInfoManager,
            IEntityFactory entityFactory,
            IFactory<ISpellParameters> spellParametersFactory,
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
            : base(globalQuestManager, creatureInfoManager, entityFactory, spellParametersFactory, gameTableManager, cinematicFactory)
        {
        }

        protected override void OnPhase(uint phase)
        {
            switch ((Phase)phase)
            {
                case Phase.Start:
                    Activate(Objective.Start);
                    Activate(Objective.Deathless);
                    // the Start Button and Phineas wait just inside the lobby, the exit beside the way in
                    SpawnCreature(StartButton, Lobby + new Vector3(0f, 0f, -6f));
                    SpawnCreature(Phineas, Lobby + new Vector3(4f, 0f, -8f), MathF.PI);
                    SpawnCreature(ExitPortal, Lobby + new Vector3(-4f, 0f, 2f));
                    break;
                case Phase.GatherInvulnotron:
                    Gather(Objective.GatherFirst, TriggerVolume.Room1Arena1, Room1Arena1);
                    break;
                case Phase.Invulnotron:
                    Fight(Objective.Invulnotron, Invulnotron, InvulnotronSpot, Room1Arena1);
                    break;
                case Phase.GatherGromka:
                    Gather(Objective.GatherRoom1Second, TriggerVolume.Room1Arena2, Room1Arena2);
                    break;
                case Phase.Gromka:
                    Fight(Objective.Gromka, Gromka, GromkaSpot, Room1Arena2);
                    break;
                case Phase.GatherIruki:
                    Gather(Objective.GatherRoom1Third, TriggerVolume.Room1Arena1, Room1Arena1);
                    break;
                case Phase.Iruki:
                    Fight(Objective.Iruki, Iruki, IrukiSpot, Room1Arena1);
                    break;
                case Phase.TeleporterRoom2:
                    Activate(Objective.TeleporterRoom2, 1u);
                    break;
                case Phase.GatherSeek:
                    Gather(Objective.GatherRoom2First, TriggerVolume.Room2, Room2);
                    break;
                case Phase.Seek:
                    Fight(Objective.Seek, Seek, Room2Arena, Room2);
                    break;
                case Phase.GatherIcebox:
                    Gather(Objective.GatherRoom2Second, TriggerVolume.Room2, Room2);
                    break;
                case Phase.Icebox:
                    Fight(Objective.Icebox, Icebox, Room2Arena, Room2);
                    break;
                case Phase.GatherSuperInvulnotron:
                    Gather(Objective.GatherRoom2Third, TriggerVolume.Room2, Room2);
                    break;
                case Phase.SuperInvulnotron:
                    Fight(Objective.SuperInvulnotron, SuperInvulnotron, Room2Arena, Room2);
                    break;
                case Phase.TeleporterRoom3:
                    Activate(Objective.TeleporterRoom3, 1u);
                    break;
                case Phase.MeetPhineas:
                    Gather(Objective.MeetPhineas, TriggerVolume.Room3, Room3);
                    SpawnCreature(PhineasRoom3, Room3 + new Vector3(0f, 0f, 6f), MathF.PI);
                    break;
                case Phase.Wrathbone:
                    Activate(Objective.Wrathbone);
                    SpawnBoss(Wrathbone, WrathboneSpot, Room3);
                    // the Room 3 Gate shuts the fight between the meeting spot and Wrathbone
                    (Vector3 position, float yaw) = DoorOnWay(Room3, WrathboneSpot, 30f);
                    AddEncounterDoor(Room3Gate, position, yaw, Wrathbone);
                    break;
            }
        }

        /// <summary>
        /// "Gather to begin the next assessment": every player of the party in the trigger volume (15 m around its location). Counted by
        /// the script, the party is often already standing in the volume when it opens (room 2's three assessments share one).
        /// </summary>
        private void Gather(uint objectiveId, uint triggerVolume, Vector3 position)
        {
            uint party = (uint)Math.Max(1, Players.Count());
            Activate(objectiveId, party);
            gather = (objectiveId, position, party);
        }

        private (uint ObjectiveId, Vector3 Position, uint Party)? gather;

        private void Fight(uint objectiveId, uint bossId, Vector3 spot, Vector3 gather)
        {
            Activate(objectiveId);
            SpawnBoss(bossId, spot, gather);
        }

        private void SpawnBoss(uint bossId, Vector3 spot, Vector3 facing)
        {
            Vector3 direction = facing - spot;
            SpawnCreature(bossId, spot, MathF.Atan2(-direction.X, -direction.Z));
        }

        public override void OnEntityActivated(IWorldEntity entity, IPlayer activator)
        {
            if (entity.CreatureId == StartButton && (Phase)publicEvent.Phase == Phase.Start)
                Update(Objective.Start);
        }

        protected override void OnObjectiveSucceeded(uint objectiveId, IPublicEventTeam team)
        {
            switch (objectiveId)
            {
                case Objective.Start:
                    // the Start Button sends the party to the Basic Confrontation Zone
                    TeleportParty(Room1Arena1);
                    publicEvent.SetPhase(Phase.GatherInvulnotron);
                    break;
                case Objective.GatherFirst:
                    publicEvent.SetPhase(Phase.Invulnotron);
                    break;
                case Objective.Invulnotron:
                    publicEvent.SetPhase(Phase.GatherGromka);
                    break;
                case Objective.GatherRoom1Second:
                    publicEvent.SetPhase(Phase.Gromka);
                    break;
                case Objective.Gromka:
                    publicEvent.SetPhase(Phase.GatherIruki);
                    break;
                case Objective.GatherRoom1Third:
                    publicEvent.SetPhase(Phase.Iruki);
                    break;
                case Objective.Iruki:
                    publicEvent.SetPhase(Phase.TeleporterRoom2);
                    break;
                case Objective.TeleporterRoom2:
                    publicEvent.SetPhase(Phase.GatherSeek);
                    break;
                case Objective.GatherRoom2First:
                    publicEvent.SetPhase(Phase.Seek);
                    break;
                case Objective.Seek:
                    publicEvent.SetPhase(Phase.GatherIcebox);
                    break;
                case Objective.GatherRoom2Second:
                    publicEvent.SetPhase(Phase.Icebox);
                    break;
                case Objective.Icebox:
                    publicEvent.SetPhase(Phase.GatherSuperInvulnotron);
                    break;
                case Objective.GatherRoom2Third:
                    publicEvent.SetPhase(Phase.SuperInvulnotron);
                    break;
                case Objective.SuperInvulnotron:
                    publicEvent.SetPhase(Phase.TeleporterRoom3);
                    break;
                case Objective.TeleporterRoom3:
                    publicEvent.SetPhase(Phase.MeetPhineas);
                    break;
                case Objective.MeetPhineas:
                    publicEvent.SetPhase(Phase.Wrathbone);
                    break;
                case Objective.Wrathbone:
                    publicEvent.SetPhase(Phase.Done);
                    CompleteExpedition([], [], 0, 0, 0, false);
                    break;
            }
        }

        protected override void OnCheck()
        {
            if (gather is (uint objectiveId, Vector3 position, uint party))
            {
                List<IPlayer> players = Players.Where(p => p.IsAlive).ToList();
                if (players.Count > 0 && players.All(p => Vector3.Distance(p.Position, position) <= 15f))
                {
                    gather = null;
                    Update(objectiveId, (int)party);
                }
            }

            switch ((Phase)publicEvent.Phase)
            {
                // the teleporter stands on the last arena's gather spot, stepping onto it carries the party on
                case Phase.TeleporterRoom2:
                    if (teleportPending == Phase.Done && AnyPlayerNear(Room1Arena1, 5f))
                        Teleport(Phase.TeleporterRoom2, Objective.TeleporterRoom2, Room2);
                    break;
                case Phase.TeleporterRoom3:
                    if (teleportPending == Phase.Done && AnyPlayerNear(Room2, 5f))
                        Teleport(Phase.TeleporterRoom3, Objective.TeleporterRoom3, Room3);
                    break;
            }
        }

        private void Teleport(Phase phase, uint objectiveId, Vector3 destination)
        {
            teleportPending = phase;
            Delay(2d, () =>
            {
                teleportPending = Phase.Done;
                TeleportParty(destination);
                Update(objectiveId);
            });
        }

        private void TeleportParty(Vector3 destination)
        {
            int i = 0;
            foreach (IPlayer player in Players.ToList())
            {
                float angle = MathF.Tau / 5f * i++;
                player.TeleportToLocal(destination + new Vector3(MathF.Cos(angle) * 3f, 1f, MathF.Sin(angle) * 3f));
            }
        }

        public override void OnEntityDeath(IUnitEntity entity)
        {
            if (entity.CreatureId == GetCreature(Wrathbone) && (Phase)publicEvent.Phase == Phase.Wrathbone)
                Update(Objective.Wrathbone);
        }
    }
}
