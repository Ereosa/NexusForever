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

namespace NexusForever.Script.Instance.Dungeon.StormtalonsLair
{
    /// <summary>
    /// Stormtalon's Lair (public event 145, world 382): the Thundercall Pell's summoning of Stormtalon.
    /// </summary>
    /// <remarks>
    /// No packet captures exist. The order follows the objective data: survive the rush of Thundercall Pell at the entrance, defeat
    /// Blade-Wind the Invoker, eliminate Aethros to reach the summoning area, destroy Stormtalon. The optional objectives (prisoner cages,
    /// tainted flower stems, the data altar, Arcanist Breeze-Binder, Thundercall Pell kills, launch pads, storm totems, Overseer
    /// Drift-Catcher) count through the generic kill and activation handling of their target groups.
    /// Guessed: the rush being the first twelve Pell killed (its target group data is unusable), the boss adds (Static Wisps for the
    /// Invoker, Gusts of Aethros and Aethros Twisters for Aethros) and their timing, no doors (their positions aren't in the client data,
    /// the temple is a single dungeon piece).
    /// </remarks>
    [ScriptFilterOwnerId(145)]
    public class StormtalonsLairEventScript : ExpeditionEventScript
    {
        private enum Phase : uint
        {
            Rush,
            Invoker,
            Aethros,
            Stormtalon,
            Done
        }

        private static class Objective
        {
            public const uint KillInvoker     = 312;
            public const uint KillAethros     = 313;
            public const uint KillStormtalon  = 314;
            public const uint RescuePrisoners = 464;
            public const uint Rush            = 539;
            public const uint TaintedStems    = 554;
            public const uint DataAltar       = 555;
            public const uint BreezeBinder    = 556;
            public const uint KillPell        = 557;
            public const uint LaunchPads      = 559;
            public const uint StormTotems     = 562;
            public const uint DriftCatcher    = 828;
        }

        private static readonly Dictionary<uint, uint> Veteran = new()
        {
            { 17153, 34689 }, { 17154, 34690 }, { 17173, 34702 }, { 36758, 37140 }, { 34697, 37145 }, { 36862, 37144 }, { 36796, 37141 },
            { 36854, 59876 }, { 36859, 37142 }, { 36861, 37143 }, { 24474, 34711 }, { 33361, 33362 },
            { 17160, 33405 }, { 17166, 32703 }, { 17163, 33406 },
            { 36598, 67379 }, { 17316, 34707 }, { 31592, 34712 }
        };

        public const uint Invoker    = 17160;
        public const uint Aethros    = 17166;
        public const uint Stormtalon = 17163;
        private const uint StaticWisp    = 36598;
        private const uint GustOfAethros = 17316;
        private const uint AethrosTwister = 31592;

        // the Thundercall Pell of the temple, normal and veteran
        private static readonly HashSet<uint> Pell = [17153, 17154, 17173, 36758, 34697, 36862, 36796, 34689, 34690, 34702, 37140, 37145, 37144, 37141];

        protected override uint WorldId => 382;
        protected override uint InitialPhase => (uint)Phase.Rush;
        protected override IReadOnlyDictionary<uint, uint> VeteranCreatures => Veteran;

        public StormtalonsLairEventScript(
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
                case Phase.Rush:
                    Activate(Objective.Rush);
                    Activate(Objective.RescuePrisoners);
                    Activate(Objective.TaintedStems);
                    Activate(Objective.DataAltar);
                    Activate(Objective.BreezeBinder);
                    Activate(Objective.KillPell);
                    Activate(Objective.LaunchPads);
                    Activate(Objective.StormTotems);
                    Activate(Objective.DriftCatcher);
                    break;
                case Phase.Invoker:
                    Activate(Objective.KillInvoker);
                    break;
                case Phase.Aethros:
                    Activate(Objective.KillAethros);
                    break;
                case Phase.Stormtalon:
                    Activate(Objective.KillStormtalon);
                    break;
            }
        }

        protected override void OnObjectiveSucceeded(uint objectiveId, IPublicEventTeam team)
        {
            switch (objectiveId)
            {
                case Objective.Rush:
                    publicEvent.SetPhase(Phase.Invoker);
                    break;
                case Objective.KillInvoker:
                    publicEvent.SetPhase(Phase.Aethros);
                    break;
                case Objective.KillAethros:
                    publicEvent.SetPhase(Phase.Stormtalon);
                    break;
                case Objective.KillStormtalon:
                    publicEvent.SetPhase(Phase.Done);
                    // Stormtalon (Normal) and (Prime), Immortal: Stormtalon's Lair (Prime) without deaths
                    CompleteExpedition([], [], 2265, 2266, 2274, true);
                    break;
            }
        }

        public override void OnEntityDeath(IUnitEntity entity)
        {
            uint creatureId = entity.CreatureId;

            // a Static Wisp spawns where each Channeler falls, the last one ends the Invoker's immunity
            if (channelerPhase && creatureId == GetCreature(Channeler))
            {
                SpawnCreature(StaticWisp, entity.Position, 0f, AggroNearestPlayer);
                if (!GetEntities<IUnitEntity>(Channeler).Any(c => c.IsAlive && c != entity))
                    EndChannelerPhase();
            }

            if (creatureId == GetCreature(GustOfAethros) && gusts.Count > 0 && gusts.All(g => g == entity || !g.IsAlive))
            {
                gusts.Clear();
                if (mapInstance.GetEntity<IUnitEntity>(aethrosGuid) is IUnitEntity aethros)
                    aethros.InvokeScriptCollection<Script.StormtalonsLairAethrosAI>(s => s.OnGustsDead());
            }

            if ((Phase)publicEvent.Phase == Phase.Rush && Pell.Contains(creatureId))
                Update(Objective.Rush);

            // the bosses' own achievements, Normal and Prime
            if (creatureId == GetCreature(Invoker))
                GrantBossAchievement(2261, 2262);
            else if (creatureId == GetCreature(Aethros))
                GrantBossAchievement(2263, 2264);
            else if (creatureId == GetCreature(Stormtalon) && (Phase)publicEvent.Phase == Phase.Stormtalon)
                Update(Objective.KillStormtalon);
        }

        private void GrantBossAchievement(ushort normal, ushort prime)
        {
            ushort achievementId = IsVeteran ? prime : normal;
            foreach (IPlayer player in Players)
                if (!player.AchievementManager.HasCompletedAchievement(achievementId))
                    player.AchievementManager.GrantAchievement(achievementId);
        }

        private const uint Channeler      = 17173;
        private const uint StaticBarrier  = 26613;
        private bool channelerPhase;
        private readonly List<IUnitEntity> gusts = [];
        private uint aethrosGuid;
        private readonly List<IUnitEntity> twisters = [];

        /// <summary>
        /// Blade-Wind the Invoker at 55%: the Thundercall Channelers in his room raise their Static Barrier, he is immune until they fall.
        /// </summary>
        public void OnInvokerChannelers(IWorldEntity invoker)
        {
            channelerPhase = true;
            List<IUnitEntity> channelers = GetEntities<IUnitEntity>(Channeler).Where(c => c.IsAlive).ToList();
            foreach (IUnitEntity channeler in channelers)
                CastOnSelf(channeler, StaticBarrier);
            if (channelers.Count == 0)
                EndChannelerPhase();
        }

        private void EndChannelerPhase()
        {
            channelerPhase = false;
            foreach (IUnitEntity invoker in GetEntities<IUnitEntity>(Invoker))
                invoker.InvokeScriptCollection<Script.StormtalonsLairInvokerAI>(s => s.OnChannelersDead());
        }

        /// <summary>
        /// Aethros' Air Shift: he dissipates into three Gusts of Aethros.
        /// </summary>
        public void OnAethrosAirShift(IWorldEntity aethros)
        {
            aethrosGuid = aethros.Guid;
            gusts.Clear();
            for (int i = 0; i < 3; i++)
            {
                float angle = MathF.Tau / 3f * i;
                Vector3 position = aethros.Position + new Vector3(MathF.Cos(angle) * 12f, 0f, MathF.Sin(angle) * 12f);
                SpawnCreature(GustOfAethros, position, angle + MathF.PI, (map, guid, vector) =>
                {
                    if (mapInstance.GetEntity<IUnitEntity>(guid) is IUnitEntity gust)
                        gusts.Add(gust);
                    AggroNearestPlayer(map, guid, vector);
                });
            }
        }

        /// <summary>
        /// Aethros' Tempest: tornadoes cross the room back and forth (for the cast, or for good in his last phase).
        /// </summary>
        public void OnAethrosTempest(IWorldEntity aethros, bool lastPhase)
        {
            foreach (IUnitEntity twister in twisters)
                twister.RemoveFromMap();
            twisters.Clear();

            for (int i = 0; i < 3; i++)
            {
                float offset = (i - 1) * 12f;
                Vector3 a = aethros.Position + new Vector3(-30f, 0f, offset);
                Vector3 b = aethros.Position + new Vector3(30f, 0f, offset);
                SpawnCreature(AethrosTwister, i % 2 == 0 ? a : b, 0f, (map, guid, vector) =>
                {
                    if (mapInstance.GetEntity<IUnitEntity>(guid) is not IUnitEntity twister)
                        return;
                    twisters.Add(twister);
                    var path = new List<Vector3>();
                    for (int n = 0; n < 12; n++)
                        path.Add(n % 2 == 0 ? (vector == a ? b : a) : (vector == a ? a : b));
                    Walk(twister, path, 6f);
                });
            }

            if (!lastPhase)
                Delay(20d, () =>
                {
                    foreach (IUnitEntity twister in twisters)
                        twister.RemoveFromMap();
                    twisters.Clear();
                });
        }

        private void SpawnAround(IWorldEntity source, uint[] creatures, float radius)
        {
            for (int i = 0; i < creatures.Length; i++)
            {
                float angle = MathF.Tau / creatures.Length * i + Random.Shared.NextSingle();
                Vector3 position = source.Position + new Vector3(MathF.Cos(angle) * radius, 0f, MathF.Sin(angle) * radius);
                SpawnCreature(creatures[i], position, angle + MathF.PI, AggroNearestPlayer);
            }
        }
    }
}
