using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Tutorial.Script.Part2
{
    [ScriptFilterOwnerId(10518)]
    public class FaceOfTheEnemyExileQuestScript : TutorialQuestScript
    {
        private const uint TurretObjectiveId = 21288;
        private static readonly uint[] Legionnaires = [73492, 73567, 73492];

        // centre of the legionnaire objective area (WorldLocation2 51740)
        private static readonly Vector3 LegionnaireArea = new(-142.066f, -860.46375f, 556.809f);

        public override QuestId? NextQuestId => QuestId.AClaimToStakeExile;

        #region Dependency Injection

        private readonly ICreatureInfoManager creatureInfoManager;

        public FaceOfTheEnemyExileQuestScript(
            ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IQuestObjective"/> is updated for <see cref="IQuest"/>.
        /// </summary>
        /// <remarks>
        /// The legionnaires aren't in the world database, spawn them once both turrets are destroyed.
        /// </remarks>
        public override void OnObjectiveUpdate(IQuestObjective objective)
        {
            base.OnObjectiveUpdate(objective);

            if (objective.ObjectiveInfo.Id != TurretObjectiveId || !objective.IsComplete())
                return;

            IPlayer player = owner.GetOwner();
            for (int i = 0; i < Legionnaires.Length; i++)
            {
                ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(Legionnaires[i]);
                if (creatureInfo == null)
                    continue;

                float angle    = MathF.Tau / Legionnaires.Length * i;
                var position   = LegionnaireArea + new Vector3(MathF.Cos(angle) * 6f, 0f, MathF.Sin(angle) * 6f);
                IWorldEntity legionnaire = player.SummonFactory.Summon(creatureInfo, position, new Vector3(angle + MathF.PI, 0f, 0f));
                TutorialFaction.MakeHostile(legionnaire);
            }
        }
    }
}
