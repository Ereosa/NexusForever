using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Reputation;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Adventure.LevianBay
{
    /// <summary>
    /// The Levian Bay adventure, Bay of Betrayal (public event 673, world 3176): Calidor Antevian tests his acolytes for a new right hand, the
    /// players race two rival teams through his trials in disguise.
    /// </summary>
    /// <remarks>
    /// No packet captures and no archived sightings exist. Built from the client data (sub-events, objectives, creatures, world locations,
    /// the QuestDirection route) and the 2015 QueueTimes walkthrough: Herald Anku'mar's speech on the shore (672), the first hallway and Team
    /// Alpha at Seaspine Point, the Grand Evocator's Trial of Will (674, three waves of augmented malverine), the Trial of Cunning (675,
    /// Varmintron and Gopherzoid dig the way down), Sublevel Theta's Charging Station (695, the security sentinels' batteries charge the
    /// teleporter), the Trial of Endurance (676, the Elder Vault gauntlet), then Calidor Antevian in Security Hub Sigma.
    /// Guessed: the rooms' places (world location groups), the waves' make-up, the teleports between the rooms standing in for the
    /// hallways, Team Beta on the hall's balcony, the rival teams counting as "competition" once all their members are dead, the trials in
    /// a fixed order (the client offers no vote for them).
    /// </remarks>
    [ScriptFilterOwnerId(673)]
    public class LevianBayEventScript : AdventureEventScript
    {
        private const uint Calidor = 67701;
        private const uint Varmintron = 68206;
        private const uint Gopherzoid = 68207;
        private const uint SecuritySentinel = 67932;

        private static readonly uint[] Order = [672, 674, 675, 695, 676];

        private static readonly Dictionary<uint, uint> TrialObjectives = new()
        {
            { 674, 4713 }, // Complete the Trial of Will
            { 675, 4714 }, // Complete the Trial of Cunning
            { 676, 4715 }  // Complete the Trial of Endurance
        };

        // the rival teams of acolytes: Team Alpha at Seaspine Point, Team Beta on the hall's balcony
        private static readonly uint[] TeamAlpha = [67574, 67580, 67582, 67584, 67590];
        private static readonly uint[] TeamBeta = [67576, 67579, 67583, 67588, 67593];

        // the Grand Evocator's augmented malverine, three waves from the hall's anchors (WorldLocation2 49324 to 49335)
        private static readonly uint[][] MalverineWaves = [[68337, 68337, 68337, 68337], [68325, 68326, 68337, 68337], [67700, 68325, 68326, 67700]];

        private const uint WaveAnchorFirst = 49324;
        private const uint SublevelTheta = 48420; // the canimids' tunnel comes out here
        private const uint ElderVault = 49568;    // Teleport Pad - Elder Vault - Entrance
        private const uint SecurityHubSigma = 48409;

        protected override uint WorldId => 3176;
        protected override uint IntroEvent => Order[0];
        protected override uint[][] Storylines => [];
        protected override uint[] FinalObjectives => [4358]; // Survive the Trials

        private bool finale;
        private int wave;
        private readonly List<IUnitEntity> waveUnits = [];

        public LevianBayEventScript(
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

        // the malverine waves, the bosses' control and the teleporter's charge are driven here
        protected override bool IsScriptDriven(uint objectiveId) => objectiveId is 4364 or 4366 or 4433 or 4440;

        protected override void OnSubEventStarted(uint id)
        {
            switch (id)
            {
                case 674:
                    wave = 0;
                    // the hallways' layout isn't known: players still on the shore after a while are brought to the Evocator's hall
                    Delay(120d, () =>
                    {
                        if (CurrentSubEvent?.Id == 674 && !AnyPlayerNear(GetLocation(49430), 60f))
                            TeleportAll(49430);
                    });
                    break;
                case 695:
                    // through the canimids' tunnel down to Sublevel Theta
                    TeleportAll(SublevelTheta);
                    break;
                case 676:
                    TeleportAll(ElderVault);
                    break;
            }
        }

        private void TeleportAll(uint worldLocationId)
        {
            Vector3 position = GetLocation(worldLocationId);
            int i = 0;
            foreach (IPlayer player in Players.ToList())
            {
                float angle = MathF.Tau / 5f * i++;
                player.TeleportToLocal(position + new Vector3(MathF.Cos(angle) * 2f, 0.5f, MathF.Sin(angle) * 2f));
            }
        }

        /// <summary>
        /// A wave of augmented malverine bursts out of the hall's anchors.
        /// </summary>
        private void SpawnWave()
        {
            waveUnits.Clear();
            uint[] creatures = MalverineWaves[wave];
            for (int i = 0; i < creatures.Length; i++)
            {
                Vector3 anchor = GetLocation(WaveAnchorFirst + (uint)((wave * 4 + i) % 12));
                if (SpawnCreature(creatures[i], anchor, 0f, AggroNearestPlayer) is IUnitEntity unit)
                    waveUnits.Add(unit);
            }
        }

        protected override void NextSubEvent(uint finishedId)
        {
            if (finishedId == Order[0])
            {
                Activate(4358);
                Activate(4713);
                Activate(4714);
                Activate(4715);
                Activate(4360);
                Activate(4361);
            }

            if (TrialObjectives.TryGetValue(finishedId, out uint trial))
                Update(trial);

            int next = Array.IndexOf(Order, finishedId) + 1;
            if (next > 0 && next < Order.Length)
            {
                StartSubEvent(Order[next]);
                return;
            }

            // the trials are survived: Calidor Antevian waits in Security Hub Sigma
            finale = true;
            TeleportAll(SecurityHubSigma);
            Activate(4652);
            Activate(4689);
            Delay(38d, () =>
            {
                Update(4652);
                Update(4689);
                Activate(4359);
                foreach (IUnitEntity calidor in GetEntities<IUnitEntity>(Calidor).Where(c => c.IsAlive))
                    AggroNearestPlayer(mapInstance, calidor.Guid, calidor.Position);
            });
        }

        public override void OnEntityDeath(IUnitEntity entity)
        {
            uint id = entity.CreatureId;

            if (finale && id == Calidor)
            {
                Update(4359);
                FinishAdventure();
                return;
            }

            // the competition: a rival team is out once all its members are dead
            if (TeamAlpha.Contains(id) && TeamAlpha.SelectMany(GetEntities<IUnitEntity>).All(u => u == entity || !u.IsAlive))
                Update(4360);
            if (TeamBeta.Contains(id) && TeamBeta.SelectMany(GetEntities<IUnitEntity>).All(u => u == entity || !u.IsAlive))
                Update(4361);

            base.OnEntityDeath(entity);

            switch (CurrentSubEvent?.Id)
            {
                case 674 when waveUnits.Contains(entity) && waveUnits.All(u => u == entity || !u.IsAlive):
                    UpdateSubObjective(4364);
                    if (++wave < MalverineWaves.Length)
                        Delay(5d, SpawnWave);
                    break;
                // Varmintron and Gopherzoid are brought down together, then their burrowing opens the way to Sublevel Theta
                case 675 when (id == Varmintron || id == Gopherzoid)
                    && GetEntities<IUnitEntity>(Varmintron).Concat(GetEntities<IUnitEntity>(Gopherzoid)).All(u => u == entity || !u.IsAlive):
                    UpdateSubObjective(4366);
                    break;
                // the security sentinels drop the batteries that charge the teleporter
                case 695 when id == SecuritySentinel:
                    UpdateSubObjective(4433);
                    if (GetEntities<IUnitEntity>(SecuritySentinel).All(u => u == entity || !u.IsAlive))
                        UpdateSubObjective(4440, 100);
                    break;
            }
        }

        public override void OnEntityActivated(IWorldEntity entity, IPlayer activator)
        {
            // the Grand Evocator starts the Trial of Will once spoken to
            if (entity.CreatureId == 67704 && CurrentSubEvent?.Id == 674 && wave == 0 && waveUnits.Count == 0)
                SpawnWave();
        }

        protected override void OnCheck()
        {
            // the Trial of Will also starts when the players reach the Evocator's hall without speaking to him
            if (CurrentSubEvent?.Id != 674 || wave != 0 || waveUnits.Count > 0)
                return;

            if (AnyPlayerNear(GetLocation(49430), 25f))
                SpawnWave();
        }
    }
}
