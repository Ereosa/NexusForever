using System.Numerics;
using System.Text.RegularExpressions;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Reputation;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Adventure.Farside
{
    /// <summary>
    /// Battlebots, the Farside adventure (public event 579, world 3010): survivors hold out in a bio-dome on Farside against the Megadroids
    /// of an ancient AI, strike back at the AI's ten avatars' bases and finally face the AI in its core.
    /// </summary>
    /// <remarks>
    /// No captures and no archived sightings exist, everything is placed at the client's world locations: the Bio-Dome (zone 4279) and its
    /// west, north and south entrances (46809, 46808, 46810), the ten avatars' bases (the ten groups of world locations around the dome), the
    /// AI Core (4288, 46476 for the Ancient AI).
    /// Built from the sub-events: The End Is Near (578), then three days, each an assault on one entrance by one kind of Megadroid unit
    /// (617 to 619, one attack type picked), a crisis inside the dome (breaches, plague, infiltrators, cyber attack, drills: 622 to 640) and a
    /// counter-assault on one avatar's base by the smugglers' vessel (607 to 616), then the Ancient AI (620).
    /// Guessed: three days, the unit make-up of each attack type, which world location group is which avatar's base, the survivors' places,
    /// the training, building and resource systems of the original (Untrained Survivors, Scrap, Fuel) left out.
    /// </remarks>
    [ScriptFilterOwnerId(579)]
    public class FarsideEventScript : AdventureEventScript
    {
        private const uint Intro = 578;
        private const uint AncientAI = 620;
        private const int Days = 3;

        private const uint LifeSupport = 63137;
        private const uint MainAI = 62911;

        private static readonly uint[] Entrances = [617, 618, 619];          // west, north, south
        private static readonly uint[] EntranceLocations = [46809, 46808, 46810];
        private static readonly uint[] Crises = [622, 624, 625, 626, 640];
        private static readonly uint[] Bases = [607, 608, 609, 610, 611, 612, 613, 614, 615, 616];
        private static readonly uint[] Avatars = [62998, 63000, 62999, 63002, 63004, 63005, 63006, 63008, 63007, 63009];

        // the ten avatars' bases, one world location of each group around the dome
        private static readonly uint[] BaseLocations = [46382, 46386, 46390, 46394, 46398, 46407, 46379, 46376, 46408, 46410];

        // Megadroid units by attack type
        private static readonly Dictionary<string, uint[]> AttackTypes = new()
        {
            { "Infantry",      [65000, 65000, 65000, 65000, 65026] },
            { "Aerial",        [65568, 65568, 65050, 65013] },
            { "Hybrid",        [65000, 65568, 65004, 65005] },
            { "Brute Force",   [65004, 65004, 65023] },
            { "Swarm",         [65005, 65005, 65005, 65005, 65005, 65005] },
            { "Mechanized",    [65023, 65023, 65026] },
            { "Subterranean",  [65260, 65008, 65260] },
            { "Infiltrator",   [65019, 65019, 65019] },
            { "Biological",    [65000, 65005, 65005] },
            { "Cyber",         [65200, 65200, 65200] }
        };

        private static readonly uint[] ExileSurvivors = [62694, 62695, 62696, 62697, 62698, 62699, 62700, 62701];
        private static readonly uint[] DominionSurvivors = [62702, 62703, 62704, 62705, 62706, 62707, 62708];

        protected override uint WorldId => 3010;
        protected override uint IntroEvent => Intro;
        protected override uint[][] Storylines => [];
        protected override uint[] FinalObjectives => [2837];

        private readonly Queue<uint> plan = new();
        private string attackType = "Infantry";
        private int day;
        private bool setUp;

        public FarsideEventScript(
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

        // the introduction and the boarding of the smugglers' vessel are driven here
        protected override bool IsScriptDriven(uint objectiveId) => objectiveId is 2845 or (>= 2931 and <= 2940);

        protected override double StallTime(uint subEventId) => 360d;

        /// <summary>
        /// One attack type per assault.
        /// </summary>
        protected override List<PublicEventObjectiveEntry> SelectObjectives(uint subEventId, List<PublicEventObjectiveEntry> mainObjectives)
        {
            if (!Entrances.Contains(subEventId) && subEventId != 622)
                return mainObjectives;

            PublicEventObjectiveEntry objective = mainObjectives[Random.Shared.Next(mainObjectives.Count)];
            Match match = Regex.Match(GetText(objective.LocalizedTextId), "Defeat All (.+?) Units");
            attackType = match.Success && AttackTypes.ContainsKey(match.Groups[1].Value) ? match.Groups[1].Value : "Infantry";
            return [objective];
        }

        protected override void OnSubEventStarted(uint id)
        {
            if (id == Intro)
            {
                SetUpDome();
                Delay(20d, () => UpdateSubObjective(2845)); // Complete the Introduction
                return;
            }

            int entrance = Array.IndexOf(Entrances, id);
            if (entrance >= 0)
            {
                // a double column through the entrance, on to the life support system
                SpawnAttack(AttackTypes[attackType], GetLocation(EntranceLocations[entrance]));
                SpawnAttack(AttackTypes[attackType], GetLocation(EntranceLocations[entrance]) + new Vector3(0f, 0f, 12f));
                return;
            }

            switch (id)
            {
                case 622:
                    SpawnAttack(AttackTypes[attackType], DomeCentre);
                    break;
                case 625:
                    SpawnAttack(AttackTypes["Infiltrator"], DomeCentre);
                    break;
                case 626:
                    SpawnAttack(AttackTypes["Cyber"], DomeCentre);
                    break;
                case 640:
                    SpawnAttack(AttackTypes["Subterranean"], DomeCentre);
                    break;
                case 624:
                    SpawnAttack(AttackTypes["Biological"], DomeCentre);
                    break;
                case AncientAI:
                    // the Ancient AI (Main AI, the final boss target group) speaks, then turns on the players
                    TeleportAll(GetLocation(46474));
                    IWorldEntity ai = SpawnCreature(MainAI, GetLocation(46476), 0f);
                    Delay(30d, () =>
                    {
                        if (ai is not IUnitEntity unit || !unit.InWorld)
                            return;
                        unit.SetFaction((Faction)218); // hostile to all
                        AggroNearestPlayer(mapInstance, unit.Guid, unit.Position);
                    });
                    break;
            }

            int baseIndex = Array.IndexOf(Bases, id);
            if (baseIndex >= 0)
            {
                // the smugglers' vessel carries the strike team to the avatar's base
                Vector3 target = GetLocation(BaseLocations[baseIndex]);
                TeleportAll(target + new Vector3(0f, 1f, 30f));
                UpdateSubObjective(2931 + (uint)baseIndex);
                SpawnAttack(AttackTypes["Mechanized"], target);
                SpawnCreature(Avatars[baseIndex], target, 0f, AggroNearestPlayer);
            }
        }

        private Vector3 DomeCentre => GetLocation(46996);

        private void SetUpDome()
        {
            if (setUp)
                return;

            setUp = true;
            SpawnCreature(LifeSupport, GetLocation(46932), 0f);

            uint[] survivors = Players.FirstOrDefault()?.Faction1 == Faction.Dominion ? DominionSurvivors : ExileSurvivors;
            uint[] spots = [46617, 46930, 46931, 46933, 46937, 46939, 46942, 46944];
            for (int i = 0; i < spots.Length; i++)
                SpawnCreature(survivors[i % survivors.Length], GetLocation(spots[i]), (float)(Random.Shared.NextDouble() * MathF.Tau));

            Activate(2837);
            Activate(2900);
        }

        private void SpawnAttack(uint[] units, Vector3 origin)
        {
            Vector3 target = GetLocation(46932); // the life support system
            for (int i = 0; i < units.Length; i++)
            {
                float angle = MathF.Tau / units.Length * i;
                Vector3 position = Surface(origin.X + MathF.Cos(angle) * 5f, origin.Z + MathF.Sin(angle) * 5f, origin.Y);
                SpawnCreature(units[i], position, angle, (map, guid, _) =>
                {
                    if (mapInstance.GetEntity<IUnitEntity>(guid) is IUnitEntity unit)
                    {
                        Walk(unit, [target], 6f);
                        AggroNearestPlayer(map, guid, unit.Position);
                    }
                });
            }
        }

        private void TeleportAll(Vector3 position)
        {
            int i = 0;
            foreach (IPlayer player in Players.ToList())
            {
                float angle = MathF.Tau / 5f * i++;
                player.TeleportToLocal(position + new Vector3(MathF.Cos(angle) * 2f, 0.5f, MathF.Sin(angle) * 2f));
            }
        }

        protected override void NextSubEvent(uint finishedId)
        {
            if (finishedId == AncientAI)
            {
                Update(2837);
                FinishAdventure();
                return;
            }

            // back to the dome after a counter-assault
            if (Bases.Contains(finishedId))
                TeleportAll(DomeCentre);

            if (plan.Count == 0)
            {
                if (day >= Days)
                {
                    StartSubEvent(AncientAI);
                    return;
                }

                // a new day: an assault on an entrance, a crisis in the dome, a counter-assault on a base
                day++;
                plan.Enqueue(Entrances[Random.Shared.Next(Entrances.Length)]);
                plan.Enqueue(Crises[Random.Shared.Next(Crises.Length)]);
                plan.Enqueue(Bases.Where(b => b != finishedId).ElementAt(Random.Shared.Next(Bases.Length - 1)));
            }

            StartSubEvent(plan.Dequeue());
        }
    }
}
