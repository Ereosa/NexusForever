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
    /// The colony is run with the original's three resources (objectives 2620 to 2622): Untrained Survivors, Scrap and Fuel. Talking to a
    /// trainer turns a survivor and some scrap into a unit (the "Player Controlled Units" and "Non-Combat Units" creatures): the soldier
    /// trainer's infantry guards an entrance, the settler trainer's builders raise barricades and traps at the entrances, the scientist
    /// trainer's doctors cure the plague, the explorer trainer's scouts bring back scrap and fuel every day, the smuggler pilots fly the
    /// counter-assaults (which also burn fuel). Every day new survivors find their way to the dome.
    /// Guessed (the adventure never went live, nothing of it was ever recorded): three days, the unit make-up of each attack type, which
    /// world location group is which avatar's base, the survivors' and trainers' places, every cost and yield of the colony.
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

        // the colony's resources and units
        private const uint UntrainedSurvivors = 2620;
        private const uint Scrap = 2621;
        private const uint Fuel = 2622;

        private enum Trainer { Soldier, Settler, Scientist, Explorer, Smuggler }

        private static readonly Dictionary<uint, Trainer> Trainers = new()
        {
            { 62551, Trainer.Soldier }, { 62555, Trainer.Soldier }, { 62550, Trainer.Settler }, { 62554, Trainer.Settler },
            { 62552, Trainer.Scientist }, { 62556, Trainer.Scientist }, { 62553, Trainer.Explorer }, { 62557, Trainer.Explorer },
            { 62622, Trainer.Smuggler }, { 62634, Trainer.Smuggler }
        };

        private readonly Dictionary<string, int> resources = new() { { "survivors", 6 }, { "scrap", 60 }, { "fuel", 20 } };
        private readonly Dictionary<Trainer, int> units = [];
        private bool dominion;

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
                    // the colony's doctors cure the plague
                    if (units.GetValueOrDefault(Trainer.Scientist) > 0)
                        Delay(20d, () => UpdateSubObjective(2834));
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

            // the colony: its trainers in the dome and its starting stock
            dominion = Players.FirstOrDefault()?.Faction1 == Faction.Dominion;
            (uint creature, uint location)[] trainers = dominion
                ? [(62555, 46617), (62554, 46930), (62556, 46931), (62557, 46933), (62634, 46937)]
                : [(62551, 46617), (62550, 46930), (62552, 46931), (62553, 46933), (62622, 46937)];
            foreach ((uint creature, uint location) in trainers)
                SpawnCreature(creature, GetLocation(location) + new Vector3(2f, 0f, 2f), 0f);

            Activate(UntrainedSurvivors, 999u);
            Activate(Scrap, 999u);
            Activate(Fuel, 999u);
            Update(UntrainedSurvivors, resources["survivors"]);
            Update(Scrap, resources["scrap"]);
            Update(Fuel, resources["fuel"]);
        }

        private bool Spend(string resource, int amount)
        {
            if (resources[resource] < amount)
                return false;

            resources[resource] -= amount;
            Update(resource switch { "survivors" => UntrainedSurvivors, "scrap" => Scrap, _ => Fuel }, -amount);
            return true;
        }

        private void Gain(string resource, int amount)
        {
            resources[resource] += amount;
            Update(resource switch { "survivors" => UntrainedSurvivors, "scrap" => Scrap, _ => Fuel }, amount);
        }

        /// <summary>
        /// A player asked a trainer to train a survivor: costs a survivor and scrap, the unit goes to work right away.
        /// </summary>
        public void Train(IWorldEntity trainer, IPlayer player)
        {
            if (!Trainers.TryGetValue(trainer.CreatureId, out Trainer kind))
                return;

            int scrapCost = kind switch { Trainer.Soldier => 10, Trainer.Settler => 15, Trainer.Smuggler => 20, _ => 5 };
            if (resources["survivors"] < 1 || resources["scrap"] < scrapCost)
            {
                player.SendSystemMessage($"Not enough survivors or scrap (a survivor and {scrapCost} scrap needed).");
                return;
            }

            Spend("survivors", 1);
            Spend("scrap", scrapCost);
            units[kind] = units.GetValueOrDefault(kind) + 1;

            Vector3 entrance = GetLocation(EntranceLocations[Random.Shared.Next(EntranceLocations.Length)]);
            switch (kind)
            {
                case Trainer.Soldier:
                    SpawnCreature(dominion ? 62509u : 62477u, entrance, 0f);
                    break;
                case Trainer.Settler:
                    SpawnCreature(63230, entrance, 0f); // Barricade
                    SpawnCreature(Random.Shared.Next(2) == 0 ? 63227u : 63229u, entrance + new Vector3(4f, 0f, 4f), 0f); // Stun or Explosive Trap
                    break;
                case Trainer.Scientist:
                    SpawnCreature(dominion ? 62638u : 62626u, DomeCentre + new Vector3(3f, 0f, 3f), 0f); // Doctor
                    break;
                case Trainer.Explorer:
                    SpawnCreature(dominion ? 62639u : 62627u, DomeCentre + new Vector3(-3f, 0f, 3f), 0f); // Scout
                    break;
                case Trainer.Smuggler:
                    SpawnCreature(dominion ? 63298u : 63297u, DomeCentre + new Vector3(3f, 0f, -3f), 0f); // Smuggler Pilot
                    break;
            }

            player.SendSystemMessage($"{kind} trained. Survivors {resources["survivors"]}, scrap {resources["scrap"]}, fuel {resources["fuel"]}.");
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

                // a new day: survivors reach the dome, the scouts bring back scrap and fuel, then an assault on an entrance, a crisis
                // in the dome and, with a smuggler pilot and fuel, a counter-assault on a base
                day++;
                Gain("survivors", 3);
                Gain("scrap", 15 + 10 * units.GetValueOrDefault(Trainer.Explorer));
                Gain("fuel", 5 + 5 * units.GetValueOrDefault(Trainer.Explorer));
                plan.Enqueue(Entrances[Random.Shared.Next(Entrances.Length)]);
                plan.Enqueue(Crises[Random.Shared.Next(Crises.Length)]);
                if (units.GetValueOrDefault(Trainer.Smuggler) > 0 && Spend("fuel", 10))
                    plan.Enqueue(Bases.Where(b => b != finishedId).ElementAt(Random.Shared.Next(Bases.Length - 1)));
            }

            StartSubEvent(plan.Dequeue());
        }
    }
}
