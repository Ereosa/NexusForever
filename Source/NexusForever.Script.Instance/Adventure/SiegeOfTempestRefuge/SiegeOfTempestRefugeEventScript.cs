using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Adventure.SiegeOfTempestRefuge
{
    /// <summary>
    /// The Siege of Tempest Refuge (world 1233): Exiles defend the refuge's generator against the Dominion (public event 173), Dominion
    /// players against the Exiles (public event 174).
    /// </summary>
    /// <remarks>
    /// Built from the client data: cross the bridge to Skywatch under the artillery barrage, the debriefing at the generator, position the
    /// troops, then five assaults, each one of the enemy's ten divisions (sub-events, picked at random without repeats) with a regroup
    /// between them, then fall back to the generator and hold out against the last assault.
    /// Guessed: the division order, each division's troops and commander (by their boss creatures' roles), two waves per assault, regroups and troop positioning finishing on short timers (their troop commands were scripted on retail),
    /// which bridge and division sub-events belong to which side (their parent links contradict their texts, the texts were followed).
    /// </remarks>
    [ScriptFilterOwnerId(173, 174)]
    public class SiegeOfTempestRefugeEventScript : AdventureEventScript
    {
        private const uint Regroup = 78;
        private const uint PositionTroops = 204;
        private const int Assaults = 5;

        private bool IsExile => publicEvent.Id == 173u;

        protected override uint WorldId => 1233;
        protected override uint IntroEvent => IsExile ? 83u : 84u;  // Cross the Bridge to Skywatch
        private uint Debriefing => IsExile ? 90u : 91u;             // Kit Brinny and Commander Kriton, Lieutenant Feraakh and Colonel Audax

        // the enemy's divisions: Dominion divisions against Exiles, FCON and Exile brigades against the Dominion
        private IEnumerable<uint> Divisions => Enumerable.Range(IsExile ? 57 : 67, 10).Select(i => (uint)i);

        protected override uint[][] Storylines => [];
        protected override uint[] FinalObjectives => [];

        private readonly Queue<uint> assaults = new();

        // where the assaults come from and what they're after (densest Jabbithole sightings of the attackers, the generator)
        private static readonly Vector3[] AttackLanes = [new(5131f, -905.6f, -2250f), new(5007f, -905.6f, -2464f), new(5008f, -905.6f, -2330f)];
        private static readonly Vector3 Generator = new(5082.127f, -905.570f, -2373.024f); // WorldLocation2 16199

        // each division's troops and its commander, Dominion divisions 57 to 66 and Exile brigades 67 to 76 in order
        private static readonly uint[] DominionTroops = [22202, 22205, 22204, 22203, 22206, 22161, 22162, 30738];
        private static readonly uint[] ExileTroops = [21284, 21499, 21478, 21287, 21512, 21286, 21292, 30725];
        private static readonly uint[] DominionCommanders = [22244, 22172, 22167, 22199, 22168, 22171, 22170, 22166, 22169, 22176];
        private static readonly uint[] ExileCommanders = [21544, 21545, 21546, 21547, 21549, 21552, 21551, 21557, 21550, 21553];

        public SiegeOfTempestRefugeEventScript(
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

        /// <summary>
        /// A division attacks: two waves of its troops down the lanes, its commander with the second.
        /// </summary>
        protected override void OnSubEventStarted(uint id)
        {
            bool dominion = id is >= 57 and <= 66;
            bool exile = id is >= 67 and <= 76;
            if (!dominion && !exile)
                return;

            uint[] troops = dominion ? DominionTroops : ExileTroops;
            uint commander = (dominion ? DominionCommanders : ExileCommanders)[id - (dominion ? 57u : 67u)];
            SpawnAssault(troops, 0u);
            Delay(30d, () => SpawnAssault(troops, commander));
        }

        private void SpawnAssault(uint[] troops, uint commander)
        {
            Vector3 lane = AttackLanes[Random.Shared.Next(AttackLanes.Length)];
            for (int i = 0; i < 6; i++)
            {
                float angle = MathF.Tau / 6f * i;
                Vector3 position = Surface(lane.X + MathF.Cos(angle) * 4f, lane.Z + MathF.Sin(angle) * 4f, lane.Y);
                SpawnCreature(troops[Random.Shared.Next(troops.Length)], position, 0f, (map, guid, _) => Charge(guid));
            }

            if (commander != 0u)
                SpawnCreature(commander, Surface(lane.X, lane.Z, lane.Y), 0f, (map, guid, _) => Charge(guid));
        }

        /// <summary>
        /// Attackers march on the generator and fight whoever they meet.
        /// </summary>
        private void Charge(uint guid)
        {
            if (mapInstance.GetEntity<IUnitEntity>(guid) is not IUnitEntity unit)
                return;

            Walk(unit, [Generator], 6f);
            AggroNearestPlayer(mapInstance, guid, unit.Position);
        }

        protected override double StallTime(uint subEventId) => subEventId switch
        {
            Regroup        => 30d,
            PositionTroops => 60d,
            _              => 480d
        };

        protected override void NextSubEvent(uint finishedId)
        {
            uint defend = IsExile ? 564u : 565u;

            if (finishedId == IntroEvent)
            {
                StartSubEvent(Debriefing);
                return;
            }

            if (finishedId == Debriefing)
            {
                StartSubEvent(PositionTroops);
                Activate(defend, Assaults);
                foreach (uint division in Divisions.OrderBy(_ => Random.Shared.Next()).Take(Assaults))
                    assaults.Enqueue(division);
                return;
            }

            if (finishedId is PositionTroops or Regroup)
            {
                StartSubEvent(assaults.Dequeue());
                return;
            }

            // an assault was beaten back
            Update(defend);
            if (assaults.Count > 0)
            {
                StartSubEvent(Regroup);
                return;
            }

            // after the last assault: fall back to the generator and hold out
            uint fallBack = IsExile ? 1703u : 1704u;
            uint holdOut = IsExile ? 2204u : 2205u;
            Activate(fallBack);
            Delay(20d, () =>
            {
                Update(fallBack);
                Activate(holdOut);
                Delay(25d, () =>
                {
                    Update(holdOut);
                    CompleteExpedition([], [], 0, 0, 0, false);
                });
            });
        }
    }
}
