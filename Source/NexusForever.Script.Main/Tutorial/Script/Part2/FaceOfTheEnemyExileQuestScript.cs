using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Reputation;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Tutorial.Script.Part2
{
    public abstract class FaceOfTheEnemyQuestScript : TutorialQuestScript, IQuestScript
    {
        protected abstract uint TurretObjectiveId { get; }
        protected abstract uint EliteObjectiveId { get; }
        protected abstract uint[] Elites { get; }
        protected abstract Vector3 EliteArea { get; }
        protected abstract Faction PlayerFaction { get; }

        private object spawnedOnMap;

        #region Dependency Injection

        private readonly ICreatureInfoManager creatureInfoManager;

        protected FaceOfTheEnemyQuestScript(
            ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        /// <remarks>
        /// The elites (legionnaires for the Exile, elite Exiles for the Dominion) aren't in the world database, they are summoned by the player once both turrets are destroyed.
        /// Summons don't survive logging out, so this also respawns them when the player returns with the objective unfinished.
        /// </remarks>
        public void Update(double lastTick)
        {
            IPlayer player = owner.GetOwner();
            if (player?.Map == null || ReferenceEquals(spawnedOnMap, player.Map))
                return;

            IQuestObjective turrets = owner.GetQuestObjective(TurretObjectiveId);
            IQuestObjective elites  = owner.GetQuestObjective(EliteObjectiveId);
            if (turrets == null || !turrets.IsComplete() || elites == null || elites.IsComplete())
                return;

            spawnedOnMap = player.Map;
            for (int i = 0; i < Elites.Length; i++)
            {
                ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(Elites[i]);
                if (creatureInfo == null)
                    continue;

                float angle  = MathF.Tau / Elites.Length * i;
                var position = EliteArea + new Vector3(MathF.Cos(angle) * 6f, 0f, MathF.Sin(angle) * 6f);
                IWorldEntity elite = player.SummonFactory.Summon(creatureInfo, position, new Vector3(angle + MathF.PI, 0f, 0f));
                TutorialFaction.MakeHostile(elite, PlayerFaction);
            }
        }
    }

    [ScriptFilterOwnerId(10518)]
    public class FaceOfTheEnemyExileQuestScript : FaceOfTheEnemyQuestScript
    {
        public override QuestId? NextQuestId => QuestId.AClaimToStakeExile;
        protected override uint TurretObjectiveId => 21288;
        protected override uint EliteObjectiveId => 21290;
        protected override uint[] Elites => [73492, 73567, 73492];
        // centre of the legionnaire objective area (WorldLocation2 51740)
        protected override Vector3 EliteArea => new(-142.066f, -860.46375f, 556.809f);
        protected override Faction PlayerFaction => Faction.Exile;

        public FaceOfTheEnemyExileQuestScript(ICreatureInfoManager creatureInfoManager)
            : base(creatureInfoManager)
        {
        }
    }

    [ScriptFilterOwnerId(10524)]
    public class FaceOfTheEnemyDominionQuestScript : FaceOfTheEnemyQuestScript
    {
        public override QuestId? NextQuestId => QuestId.AClaimToStakeDominion;
        protected override uint TurretObjectiveId => 21316;
        protected override uint EliteObjectiveId => 21317;
        // target group 14402: Virtual Elite Exile F and M
        protected override uint[] Elites => [73473, 73566, 73473];
        // centre of the elite Exile objective area (WorldLocation2 53015)
        protected override Vector3 EliteArea => new(143.45f, -854.96f, 496.92f);
        protected override Faction PlayerFaction => Faction.Dominion;

        public FaceOfTheEnemyDominionQuestScript(ICreatureInfoManager creatureInfoManager)
            : base(creatureInfoManager)
        {
        }
    }
}
