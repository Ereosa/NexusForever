using System.Text.RegularExpressions;
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

namespace NexusForever.Script.Instance.Adventure.CrimelordsOfWhitevale
{
    /// <summary>
    /// Crimelords of Whitevale (public event 146, world 1323): someone killed the Blood Scions, the players take over the gang and become the
    /// biggest gangsters in Thermock.
    /// </summary>
    /// <remarks>
    /// Built from the client data: the intro (No Place Like Home, 109), then rounds chosen by the "What's Next?" votes (30 to 33) between
    /// Vengeance, War in the Streets, Alliance and Criminal Enterprise, Alliance asking "Which Gang?" (votes 34 to 37: the Rocktown Rollers,
    /// the Sons of Ravok, the Geargrinders), the Null Point defended midway, then Loose Ends and the final onslaught at the Clubhouse.
    /// Each mission sub-event has variants (one per enemy organization, one per district): a run picks one organization and one district.
    /// Guessed: the number of rounds and the votes' order, which missions each choice and each gang leads to, the Null Point defence after
    /// the second round, missions finishing for the players after four minutes without progress.
    /// </remarks>
    [ScriptFilterOwnerId(146)]
    public class CrimelordsOfWhitevaleEventScript : AdventureEventScript, IPublicEventScript
    {
        private enum Choice { Vengeance, WarInTheStreets, Alliance, CriminalEnterprise }
        private enum Gang { RocktownRollers, SonsOfRavok, Geargrinders }

        private const uint Intro = 109;
        private const uint DefendNullPoint = 151;
        private const int Rounds = 4;

        private static readonly uint[] Enterprises = [111, 112, 113, 114, 310, 315, 317];
        private static readonly uint[] WarInTheStreets = [281, 282, 283, 284, 285, 286];
        private static readonly uint[] Vengeance = [288, 289, 290, 291, 292, 293, 295, 296];
        private static readonly uint[] LooseEnds = [367, 368, 369];
        private static readonly Dictionary<Gang, uint[]> GangMissions = new()
        {
            { Gang.RocktownRollers, [298, 299] }, // The Battle of Rocktown, Blood Feud ("Ma" Thrasher against the Rollers)
            { Gang.SonsOfRavok,     [300, 301] }, // Death Before Dishonor, Big Trouble in Little Mikros
            { Gang.Geargrinders,    [302, 294] }  // Training Day, Cornering the Market
        };

        private static readonly string[] Organizations = ["Darkspur", "Marauder|Redmoon", "Protostar"];

        protected override uint WorldId => 1323;
        protected override uint IntroEvent => Intro;
        protected override uint[][] Storylines => [];
        protected override uint[] FinalObjectives => [318]; // Become the biggest gangsters in Thermock

        private readonly List<Choice> choices = [];
        private readonly HashSet<Gang> allies = [];
        private readonly HashSet<uint> played = [];
        private uint pendingVote;
        private int round;
        private bool finale;

        public CrimelordsOfWhitevaleEventScript(
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

        protected override double StallTime(uint subEventId) => 240d;

        /// <summary>
        /// One enemy organization and one district per mission, War in the Streets' sabotage jobs one at a time.
        /// </summary>
        protected override List<PublicEventObjectiveEntry> SelectObjectives(uint subEventId, List<PublicEventObjectiveEntry> mainObjectives)
        {
            if (subEventId == 286)
                return [mainObjectives[Random.Shared.Next(mainObjectives.Count)]];

            var texts = mainObjectives.ToDictionary(o => o.Id, o => GetText(o.LocalizedTextId));
            string[] mentioned = Organizations.Where(org => texts.Values.Any(t => Regex.IsMatch(t, org))).ToArray();
            List<PublicEventObjectiveEntry> selected = mainObjectives;
            if (mentioned.Length > 1)
            {
                string org = mentioned[Random.Shared.Next(mentioned.Length)];
                selected = mainObjectives
                    .Where(o => Regex.IsMatch(texts[o.Id], org) || !mentioned.Any(m => Regex.IsMatch(texts[o.Id], m)))
                    .ToList();
            }

            // the same objective in several districts: one district per run
            return selected
                .GroupBy(o => Regex.Replace(texts[o.Id], @"\s+(in|around|at|near|beyond|aboard)\s+.*$", ""))
                .Select(g => g.ElementAt(Random.Shared.Next(g.Count())))
                .ToList();
        }

        protected override void NextSubEvent(uint finishedId)
        {
            if (finishedId == Intro)
            {
                Activate(315); // Find out who killed the Blood Scions
                Activate(318);
                NextRound();
                return;
            }

            if (finale)
            {
                if (LooseEnds.Contains(finishedId))
                {
                    // back to the Clubhouse for the final onslaught
                    Update(315);
                    Activate(1482);
                    Activate(1483);
                    Activate(1484);
                    Delay(5d, () => Update(1482));
                    Delay(60d, () =>
                    {
                        Update(1483);
                        Update(1484);
                        FinishAdventure();
                    });
                }
                return;
            }

            if (finishedId != DefendNullPoint && round == 2 && !played.Contains(DefendNullPoint))
            {
                Play(DefendNullPoint);
                return;
            }

            NextRound();
        }

        private void NextRound()
        {
            round++;
            if (round > Rounds)
            {
                finale = true;
                Play(LooseEnds[Random.Shared.Next(LooseEnds.Length)]);
                return;
            }

            bool allied = choices.Contains(Choice.Alliance) && allies.Count == 3;
            uint vote = round switch
            {
                1 => 31u,                       // Alliance or Criminal Enterprise
                2 => 30u,                       // Vengeance or War in the Streets
                3 => allied ? 33u : 32u,        // Criminal Enterprise or War, Alliance or War
                _ => 30u
            };
            StartVote(vote);
        }

        private void StartVote(uint voteId)
        {
            pendingVote = voteId;
            publicEvent.StartVote(PublicEventTeam.PublicTeam, voteId, 0u);
        }

        public new void OnVoteFinished(uint voteId, uint winner)
        {
            if (voteId != pendingVote)
                return;

            pendingVote = 0u;
            switch (voteId)
            {
                case 30u: Choose(winner == 0u ? Choice.Vengeance : Choice.WarInTheStreets); break;
                case 31u: Choose(winner == 0u ? Choice.Alliance : Choice.CriminalEnterprise); break;
                case 32u: Choose(winner == 0u ? Choice.Alliance : Choice.WarInTheStreets); break;
                case 33u: Choose(winner == 0u ? Choice.CriminalEnterprise : Choice.WarInTheStreets); break;
                case 34u: Ally((Gang)Math.Min(winner, 2u)); break;
                case 35u: Ally(winner == 0u ? Gang.RocktownRollers : Gang.SonsOfRavok); break;
                case 36u: Ally(winner == 0u ? Gang.RocktownRollers : Gang.Geargrinders); break;
                case 37u: Ally(winner == 0u ? Gang.SonsOfRavok : Gang.Geargrinders); break;
            }
        }

        private void Choose(Choice choice)
        {
            choices.Add(choice);
            switch (choice)
            {
                case Choice.Vengeance:
                    PlayFrom(Vengeance);
                    break;
                case Choice.WarInTheStreets:
                    PlayFrom(WarInTheStreets);
                    break;
                case Choice.CriminalEnterprise:
                    PlayFrom(Enterprises);
                    break;
                case Choice.Alliance:
                    // which gang to win over, the ones already allied left out
                    StartVote(allies.Count == 0 ? 34u
                        : !allies.Contains(Gang.RocktownRollers) && !allies.Contains(Gang.SonsOfRavok) ? 35u
                        : !allies.Contains(Gang.RocktownRollers) && !allies.Contains(Gang.Geargrinders) ? 36u
                        : 37u);
                    break;
            }
        }

        private void Ally(Gang gang)
        {
            allies.Add(gang);
            PlayFrom(GangMissions[gang]);
        }

        private void PlayFrom(uint[] pool)
        {
            uint[] fresh = pool.Where(p => !played.Contains(p)).ToArray();
            uint[] options = fresh.Length > 0 ? fresh : pool;
            Play(options[Random.Shared.Next(options.Length)]);
        }

        private void Play(uint subEventId)
        {
            played.Add(subEventId);
            StartSubEvent(subEventId);
        }
    }
}
