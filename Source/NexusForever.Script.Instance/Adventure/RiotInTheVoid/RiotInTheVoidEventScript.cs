using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.GameTable;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Adventure.RiotInTheVoid
{
    /// <summary>
    /// Riot in the Void (public event 179, world 1437): the prisoners of the Astrovoid Prison have risen under Jarak, Agent Vadim and Professor
    /// Goldbough, Warden Rhadman needs the strike team to secure the prison one problem at a time.
    /// </summary>
    /// <remarks>
    /// No packet captures exist. The adventure is built from its client data: the intro (public event 178), then five stages of sub-events
    /// along three storylines named in the creature descriptions, Transport (Agent Vadim), Nexus (Professor Goldbough) and Arkship (Jarak).
    /// Stage 1 and 2 are voted (votes 42 and 43), stage 3 continues the storyline chosen for stage 2, stage 4 is voted between the two other
    /// storylines (votes 44, 50 and 51 each leave one out) and stage 5 is the finale of the storyline the group followed most.
    /// Guessed: the stage 3 and stage 5 rules, kill counts standing in for the objectives that were scripted on retail ("Script" type),
    /// exterminate objectives completing when the sub-event's hostiles are dead, the timed holds and briefings ending on their timers.
    /// </remarks>
    [ScriptFilterOwnerId(179)]
    public class RiotInTheVoidEventScript : ExpeditionEventScript, IPublicEventScript
    {
        private enum Storyline { Transport, Nexus, Arkship }

        private const uint Intro = 178;

        // sub-events by storyline and stage
        private static readonly Dictionary<Storyline, uint[]> Stages = new()
        {
            { Storyline.Transport, [403, 406, 409, 412, 415] },
            { Storyline.Nexus,     [407, 413, 410, 404, 416] },
            { Storyline.Arkship,   [405, 408, 411, 414, 417] }
        };

        // vote options are in storyline order Transport, Nexus, Arkship except where noted
        private const uint VoteStage1 = 42;          // Arms Race, Felony Geology, Cold Storage
        private const uint VoteStage2 = 43;          // Shooting Gallery, Breaking Beakers, Yard Time
        private const uint VoteStage4NoTransport = 44; // The Sky Is Falling, Hidden Agendas
        private const uint VoteStage4NoNexus = 50;     // Demented Demolition, Hidden Agendas
        private const uint VoteStage4NoArkship = 51;   // Demented Demolition, The Sky Is Falling

        private static readonly Vector3 PrisonEntrance = new(-202.612f, -499.042f, -25.617f); // WorldLocation2 22758, objective 654
        private static readonly Vector3 WardenOffice   = new(-256.985f, -495.706f, 83.160f);  // WorldLocation2 39185, objective 608
        private static readonly Vector3 VoidsGate      = new(-156.618f, -524.677f, -287.499f); // WorldLocation2 38972, objective 600
        private static readonly Vector3 GoldbougYard   = new(-143.647f, -526.182f, -191.397f); // WorldLocation2 39860, objective 1721

        protected override uint WorldId => 1437;
        protected override uint InitialPhase => 0u;
        protected override IReadOnlyDictionary<uint, uint> VeteranCreatures => new Dictionary<uint, uint>();

        private readonly List<Storyline> chosen = [];
        private IPublicEvent current;
        private int stage;
        private uint pendingVote;
        private readonly Dictionary<uint, int> kills = [];
        private readonly List<IUnitEntity> hostiles = [];

        public RiotInTheVoidEventScript(
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
            if (phase != 0u)
                return;

            Activate(1714); // Quell the riot in Astrovoid Prison
            Delay(1d, () => StartSubEvent(Intro));
        }

        public override void OnAddToMap(IGridEntity entity)
        {
            base.OnAddToMap(entity);

            // late joiners join the running sub-event too
            if (entity is IPlayer player && current != null && !current.HasFinished)
                current.JoinEvent(player, PublicEventTeam.PublicTeam);

            // the running sub-event's hostiles (its entities are spawned as it starts)
            if (current != null && entity is IUnitEntity unit and not IPlayer && unit.Faction1 != Game.Static.Reputation.Faction.FriendlyToAll
                && unit.CreatureInfo?.Entry?.CreationTypeEnum == Game.Static.Entity.EntityType.NonPlayer)
                hostiles.Add(unit);
        }

        private void StartSubEvent(uint id)
        {
            hostiles.Clear();
            current = mapInstance.PublicEventManager.CreateEvent(id);
            foreach (IPlayer player in Players)
                current.JoinEvent(player, PublicEventTeam.PublicTeam);

            kills.Clear();
            switch (id)
            {
                case Intro:
                    // Report to Agent Triphon is the intro's initial objective
                    break;
                case 403: SubActivate(602); SubActivate(1738); break;
                case 404: SubActivate(1739); SubActivate(603); break;
                case 405:
                    SubActivate(1723);
                    AddSubTrigger(5767, new Vector3(-282.066f, -956.024f, 975.051f), 10f); // WorldLocation2 39187, maximum security
                    break;
                case 406: SubActivate(1766); SubActivate(604); break;
                case 407: SubActivate(605); break;
                case 408: SubActivate(601, 9u); break;
                case 409:
                    SubActivate(1718);
                    // Agent Vadim's Jabbithole page wasn't archived, he waits at the objective's location
                    Delay(1d, () => SpawnCreature(27446, Surface(-302.914f, 98.668f, -506.413f), 0f)); // WorldLocation2 38776
                    break;
                case 410:
                    SubActivate(1717);
                    // Professor Goldbough in her Support Deck lab (her creature isn't tagged with the sub-event)
                    Delay(1d, () => SpawnCreature(48517, Surface(-303.261f, 99.146f, -506.284f), 0f)); // WorldLocation2 38930
                    break;
                case 411: SubActivate(1716); break;
                case 412:
                    AddSubTrigger(5747, VoidsGate, 10f);
                    break;
                case 413: SubActivate(1730); SubActivate(2188); break;
                case 414:
                    AddSubTrigger(5775, new Vector3(-413.792f, -516.710f, 209.157f), 10f); // WorldLocation2 39186
                    break;
                case 415: SubActivate(1720); SubActivate(1897); break;
                case 416:
                    AddSubTrigger(6030, GoldbougYard, 10f);
                    break;
                case 417:
                    SubActivate(1722);
                    SubActivate(1891);
                    // Jarak's spawn belongs to Hardened Criminal (his first sub-event), the finale brings him to the Yard
                    Delay(2d, () => SpawnCreature(27445, Surface(-141.661f, -199.366f, -525.329f), 0f, AggroNearestPlayer)); // WorldLocation2 39325
                    break;
            }
        }

        private void SubActivate(uint objectiveId, uint max = 0u)
        {
            current?.ActivateObjective(objectiveId, max);
        }

        private void SubUpdate(uint objectiveId, int count = 1)
        {
            current?.UpdateObjective(objectiveId, count);
        }

        private void AddSubTrigger(uint objectId, Vector3 position, float range)
        {
            var trigger = current.CreateEntity<Game.Abstract.Entity.Trigger.IVolumeGridTriggerEntity>();
            trigger.Initialise(objectId, range, objectId);
            trigger.AddToMap(mapInstance, position);
        }

        /// <summary>
        /// An objective of the running sub-event changed status.
        /// </summary>
        public void OnSubObjective(IPublicEvent subEvent, IPublicEventObjective objective)
        {
            if (subEvent != current || objective.Status != PublicEventStatus.Succeeded)
                return;

            uint id = objective.Entry.Id;
            switch (id)
            {
                // intro: Agent Triphon, the briefing, the prison entrance, the prisoners, the warden's office, Warden Rhadman
                case 2073:
                    SubActivate(610);
                    Delay(20d, () => SubUpdate(610));
                    break;
                case 610:
                    SubActivate(654);
                    AddSubTrigger(3482, PrisonEntrance, 4f);
                    break;
                case 654:
                    SubActivate(652);
                    SubActivate(608);
                    AddSubTrigger(3383, WardenOffice, 4f);
                    break;
                case 608:
                    SubActivate(646);
                    Delay(40d, () => SubUpdate(646));
                    break;
                case 646:
                    EndSubEvent();
                    break;

                // stage objectives that lead on to the next one
                case 1723: SubActivate(1863, 20u); break;
                case 600:
                    SubActivate(1838);
                    SubActivate(1836);
                    Delay(180d, () => SubUpdate(1836));
                    break;
                case 1719:
                    SubActivate(1987);
                    SubActivate(1886);
                    Delay(180d, () => SubUpdate(1886));
                    break;
                case 1717: SubActivate(1825); break;
                case 1721:
                    SubActivate(2107, 5u);
                    SubActivate(1867);
                    SpawnCreature(48846, Surface(GoldbougYard.X, GoldbougYard.Z, GoldbougYard.Y), 0f, AggroNearestPlayer);
                    break;

                // the objectives that finish their sub-event
                case 602 or 1738 when IsDone(subEvent, 602, 1738): EndSubEvent(); break;
                case 603 or 1739 when IsDone(subEvent, 603, 1739): EndSubEvent(); break;
                case 1863: EndSubEvent(); break;
                case 604 or 1766 when IsDone(subEvent, 604, 1766): EndSubEvent(); break;
                case 605: EndSubEvent(); break;
                case 601: EndSubEvent(); break;
                case 1718: EndSubEvent(); break;
                case 1825: EndSubEvent(); break;
                case 1716: EndSubEvent(); break;
                case 1836: EndSubEvent(); break;
                case 1730 or 2188 when IsDone(subEvent, 1730, 2188): EndSubEvent(); break;
                case 1886: EndSubEvent(); break;
                case 1720 or 1897 when IsDone(subEvent, 1720, 1897): EndSubEvent(); break;
                case 1867: EndSubEvent(); break;
                case 1722 or 1891 when IsDone(subEvent, 1722, 1891): EndSubEvent(); break;
            }
        }

        private static bool IsDone(IPublicEvent subEvent, params uint[] objectives)
        {
            IPublicEventTeam team = subEvent.GetTeams().FirstOrDefault(t => t.Team == PublicEventTeam.PublicTeam);
            return team != null && IsSucceeded(team, objectives);
        }

        private void EndSubEvent()
        {
            IPublicEvent finished = current;
            current = null;
            finished?.Finish(PublicEventTeam.PublicTeam);

            stage++;
            switch (stage)
            {
                case 1:
                    StartVote(VoteStage1);
                    break;
                case 2:
                    StartVote(VoteStage2);
                    break;
                case 3:
                    // the story continues where the group went last
                    Continue(chosen[^1]);
                    break;
                case 4:
                    StartVote(chosen[^1] switch
                    {
                        Storyline.Transport => VoteStage4NoTransport,
                        Storyline.Nexus     => VoteStage4NoNexus,
                        _                   => VoteStage4NoArkship
                    });
                    break;
                case 5:
                    // the finale of the storyline followed most, the latest choice breaks ties
                    Storyline finale = chosen
                        .GroupBy(s => s)
                        .OrderByDescending(g => g.Count())
                        .ThenByDescending(g => chosen.LastIndexOf(g.Key))
                        .First().Key;
                    Continue(finale);
                    break;
                default:
                    Update(1714);
                    CompleteExpedition([], [], 0, 0, 0, false);
                    break;
            }
        }

        private void StartVote(uint voteId)
        {
            pendingVote = voteId;
            publicEvent.StartVote(PublicEventTeam.PublicTeam, voteId, 0u);
        }

        public void OnVoteFinished(uint voteId, uint winner)
        {
            if (voteId != pendingVote)
                return;

            pendingVote = 0u;
            Storyline[] options = voteId switch
            {
                VoteStage4NoTransport => [Storyline.Nexus, Storyline.Arkship],
                VoteStage4NoNexus     => [Storyline.Transport, Storyline.Arkship],
                VoteStage4NoArkship   => [Storyline.Transport, Storyline.Nexus],
                _                     => [Storyline.Transport, Storyline.Nexus, Storyline.Arkship]
            };
            Continue(options[Math.Min((int)winner, options.Length - 1)]);
        }

        private void Continue(Storyline storyline)
        {
            chosen.Add(storyline);
            StartSubEvent(Stages[storyline][stage - 1]);
        }

        protected override void OnObjectiveSucceeded(uint objectiveId, IPublicEventTeam team)
        {
        }

        public override void OnEntityDeath(IUnitEntity entity)
        {
            if (current == null)
                return;

            uint id = current.Id;
            int count = kills[id] = kills.GetValueOrDefault(id) + 1;

            // kill counts standing in for the scripted objectives
            switch (id)
            {
                case 405: SubUpdate(1863); break;
                case 408: SubUpdate(601); break;
                case 416 when entity.CreatureId == 48846: SubUpdate(1867); break;
            }

            // a sub-event's exterminate objectives finish once none of its hostiles are left
            if (hostiles.Contains(entity) && !HostilesLeft(entity))
                foreach (uint objective in ExterminateObjectives(id))
                    SubUpdate(objective);
        }

        private bool HostilesLeft(IUnitEntity dying)
        {
            return hostiles.Contains(dying) && hostiles.Any(u => u != dying && u.IsAlive && u.InWorld);
        }

        private static uint[] ExterminateObjectives(uint subEvent) => subEvent switch
        {
            403 => [1738],
            404 => [603],
            406 => [604],
            409 => [1718],
            411 => [1716],
            413 => [1730, 2188],
            415 => [1720],
            417 => [1722, 1891],
            _   => []
        };
    }
}
