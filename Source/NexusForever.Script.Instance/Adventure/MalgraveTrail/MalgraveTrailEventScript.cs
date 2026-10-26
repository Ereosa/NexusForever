using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Adventure.MalgraveTrail
{
    /// <summary>
    /// The Malgrave Trail (public event 53, world 1181): Caravan Master Braithwait leads the survivors of Thirsty Creek across Malgrave to
    /// Fort Westwatch, the players choose the caravan's way stop by stop.
    /// </summary>
    /// <remarks>
    /// Built from the client data: Exodus (56) in Thirsty Creek, then stops chosen by the "Choose Your Path" votes (2 to 19 and 22, each
    /// listing two to four destinations), each stop its own sub-event ("Gather at ..." with Braithwait, then the stop's trouble), Sunstorm
    /// Village asking which Draken clan to aid (vote 23), trouble on the trail between stops (lost caravan members, hunting, dropped
    /// supplies, fires, ambushes, thieves), and the ships that carry the caravan the last leg.
    /// Guessed: which vote follows which stop (the vote listing the nearest unvisited destinations), six stops a run, trail trouble between
    /// half the stops, the ship nearest the last stop, Braithwait waiting at each stop's gathering point, the camp votes left out.
    /// </remarks>
    [ScriptFilterOwnerId(53)]
    public class MalgraveTrailEventScript : AdventureEventScript, IPublicEventScript
    {
        private const uint Exodus = 56;
        private const uint Braithwait = 33173;
        private const uint DrakenVote = 23;
        private const uint Sunstorm = 41;
        private const int StopsPerRun = 6;

        // stops: sub-event and the world location of its gathering point
        private static readonly Dictionary<uint, uint> Stops = new()
        {
            { 103, 14230 }, { 33, 14231 }, { 34, 14232 }, { 50, 14233 }, { 41, 14234 }, { 101, 14235 }, { 38, 14236 }, { 35, 14237 },
            { 104, 14238 }, { 79, 14242 }, { 102, 14243 }, { 36, 14244 }, { 37, 14245 }, { 77, 14246 }, { 40, 14247 }, { 48, 14248 },
            { 51, 14250 }, { 55, 14251 }, { 82, 14252 }, { 54, 13667 }
        };

        // "Choose Your Path" votes and the stop of each option, in option order
        private static readonly Dictionary<uint, uint[]> PathVotes = new()
        {
            { 2, [103, 101, 102, 48] }, { 3, [33, 38] }, { 4, [34, 35] }, { 5, [50, 104] }, { 6, [41, 79] }, { 7, [38, 33] }, { 8, [35, 34] },
            { 9, [104, 50, 77] }, { 10, [79, 41, 40] }, { 11, [36, 38, 51] }, { 12, [37, 55, 77] }, { 13, [37, 55, 77] }, { 14, [77, 104, 82] },
            { 15, [40, 79, 54] }, { 16, [51, 36] }, { 17, [55, 37] }, { 18, [82, 77] }, { 19, [54, 40] }, { 22, [104, 50, 77] }
        };

        // the ships to Fort Westwatch: sub-event and its world location
        private static readonly (uint SubEvent, uint Location)[] Ships = [(45, 15595), (46, 15596), (47, 15597)];

        private static readonly uint[] TrailTrouble = [219, 220, 234, 239, 513, 514, 515, 28, 29];

        protected override uint WorldId => 1181;
        protected override uint IntroEvent => Exodus;
        protected override uint[][] Storylines => [];
        protected override uint[] FinalObjectives => [66]; // Lead the caravan to Fort Westwatch safely

        private readonly List<uint> visited = [];
        private uint pendingVote;
        private uint[] pendingOptions = [];
        private uint currentStop;
        private bool aidSunstorm;
        private bool finale;
        private IWorldEntity braithwait;

        public MalgraveTrailEventScript(
            IGlobalQuestManager globalQuestManager,
            ICreatureInfoManager creatureInfoManager,
            IEntityFactory entityFactory,
            IFactory<ISpellParameters> spellParametersFactory,
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
            : base(globalQuestManager, creatureInfoManager, entityFactory, spellParametersFactory, gameTableManager, cinematicFactory)
        {
        }

        protected override (uint VoteId, int[] Options)? GetVote(int stage, IReadOnlyList<int> chosen) => null;

        protected override double StallTime(uint subEventId) => 300d;

        /// <summary>
        /// Duplicated objectives (game versions) count once, Sunstorm Village keeps the clan the group chose to aid.
        /// </summary>
        protected override List<PublicEventObjectiveEntry> SelectObjectives(uint subEventId, List<PublicEventObjectiveEntry> mainObjectives)
        {
            if (subEventId == Sunstorm)
                mainObjectives = mainObjectives.Where(o => o.Id != (aidSunstorm ? 50u : 65u)).ToList();

            return mainObjectives
                .GroupBy(o => GetText(o.LocalizedTextId))
                .Select(g => g.First())
                .ToList();
        }

        protected override void OnSubEventStarted(uint id)
        {
            if (!Stops.TryGetValue(id, out uint location))
                return;

            // Caravan Master Braithwait waits for the caravan at the stop's gathering point
            RemoveEntity(ref braithwait);
            braithwait = SpawnCreature(Braithwait, GetLocation(location), 0f);
        }

        protected override void NextSubEvent(uint finishedId)
        {
            if (finale)
            {
                FinishAdventure();
                return;
            }

            if (finishedId == Exodus)
            {
                Activate(66);
                StartPathVote(2);
                return;
            }

            if (Stops.ContainsKey(finishedId) && Random.Shared.Next(2) == 0)
            {
                // trouble on the trail before the next stop
                StartSubEvent(TrailTrouble[Random.Shared.Next(TrailTrouble.Length)]);
                return;
            }

            if (visited.Count >= StopsPerRun || !NextVote())
            {
                // the last leg: the ship nearest the caravan
                finale = true;
                Vector3 here = GetLocation(Stops[currentStop]);
                (uint ship, _) = Ships.OrderBy(s => Vector3.DistanceSquared(GetLocation(s.Location), here)).First();
                StartSubEvent(ship);
            }
        }

        /// <summary>
        /// The vote listing the destinations nearest the caravan that it hasn't visited yet.
        /// </summary>
        private bool NextVote()
        {
            Vector3 here = GetLocation(Stops[currentStop]);
            var vote = PathVotes
                .Where(v => v.Key != 2u && v.Value.Count(s => !visited.Contains(s)) >= 2)
                .OrderBy(v => v.Value.Where(s => !visited.Contains(s)).Average(s => Vector3.Distance(GetLocation(Stops[s]), here)))
                .FirstOrDefault();
            if (vote.Value == null)
                return false;

            StartPathVote(vote.Key);
            return true;
        }

        private void StartPathVote(uint voteId)
        {
            pendingVote = voteId;
            pendingOptions = PathVotes[voteId];
            publicEvent.StartVote(PublicEventTeam.PublicTeam, voteId, 0u);
        }

        public new void OnVoteFinished(uint voteId, uint winner)
        {
            if (voteId != pendingVote)
                return;

            pendingVote = 0u;
            if (voteId == DrakenVote)
            {
                aidSunstorm = winner == 0u;
                StartSubEvent(Sunstorm);
                return;
            }

            uint stop = pendingOptions[Math.Min((int)winner, pendingOptions.Length - 1)];
            // a destination already visited: the first one that isn't
            if (visited.Contains(stop))
                stop = pendingOptions.FirstOrDefault(s => !visited.Contains(s), stop);

            visited.Add(stop);
            currentStop = stop;

            if (stop == Sunstorm)
            {
                // two Draken clans ask for help, which will the caravan aid?
                pendingVote = DrakenVote;
                publicEvent.StartVote(PublicEventTeam.PublicTeam, DrakenVote, 0u);
                return;
            }

            StartSubEvent(stop);
        }
    }
}
