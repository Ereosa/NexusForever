using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Tutorial.Script.Part2
{
    [ScriptFilterOwnerId(10518)]
    public class FaceOfTheEnemyExileQuestScript : TutorialQuestScript, IQuestScript
    {
        private const uint TurretObjectiveId      = 21288;
        private const uint LegionnaireObjectiveId = 21290;
        private static readonly uint[] Legionnaires = [73492, 73567, 73492];

        // centre of the legionnaire objective area (WorldLocation2 51740)
        private static readonly Vector3 LegionnaireArea = new(-142.066f, -860.46375f, 556.809f);

        public override QuestId? NextQuestId => QuestId.AClaimToStakeExile;

        private object spawnedOnMap;

        #region Dependency Injection

        private readonly ICreatureInfoManager creatureInfoManager;

        public FaceOfTheEnemyExileQuestScript(
            ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        /// <remarks>
        /// The legionnaires aren't in the world database, they are summoned by the player once both turrets are destroyed.
        /// Summons don't survive logging out, so this also respawns them when the player returns with the objective unfinished.
        /// </remarks>
        public void Update(double lastTick)
        {
            IPlayer player = owner.GetOwner();
            if (player?.Map == null || ReferenceEquals(spawnedOnMap, player.Map))
                return;

            IQuestObjective turrets      = owner.GetQuestObjective(TurretObjectiveId);
            IQuestObjective legionnaires = owner.GetQuestObjective(LegionnaireObjectiveId);
            if (turrets == null || !turrets.IsComplete() || legionnaires == null || legionnaires.IsComplete())
                return;

            spawnedOnMap = player.Map;
            for (int i = 0; i < Legionnaires.Length; i++)
            {
                ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(Legionnaires[i]);
                if (creatureInfo == null)
                    continue;

                float angle  = MathF.Tau / Legionnaires.Length * i;
                var position = LegionnaireArea + new Vector3(MathF.Cos(angle) * 6f, 0f, MathF.Sin(angle) * 6f);
                IWorldEntity legionnaire = player.SummonFactory.Summon(creatureInfo, position, new Vector3(angle + MathF.PI, 0f, 0f));
                TutorialFaction.MakeHostile(legionnaire);
            }
        }
    }
}
