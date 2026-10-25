using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Quest;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Spell.Effect.Handler
{
    /// <remarks>
    /// DataBits00 is the quest, DataBits01 the objective index and DataBits02 the amount.
    /// Quest objects and quest items use this for objectives that have no target of their own (objective data 0).
    /// </remarks>
    [SpellEffectHandler(SpellEffectType.QuestAdvanceObjective)]
    public class SpellEffectQuestAdvanceObjectiveHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            // effects aimed at a creature credit the player casting on it
            IPlayer player = target as IPlayer ?? executionContext.Spell.Caster as IPlayer;
            if (player == null)
                return SpellEffectExecutionResult.Ok;

            IQuestObjective objective = player.QuestManager
                .GetActiveQuest((ushort)data.DataBits00)?
                .GetQuestObjectiveByIndex((byte)data.DataBits01);
            if (objective == null || objective.IsComplete())
                return SpellEffectExecutionResult.Ok;

            uint amount = Math.Max(1u, data.DataBits02);
            if (objective.ObjectiveInfo.Type is QuestObjectiveType.ActivateTargetGroup
                or QuestObjectiveType.ActivateTargetGroupChecklist
                or QuestObjectiveType.TalkToTargetGroup)
            {
                // checklist progress is a bit mask, set the next unset bits
                uint mask = 0u;
                for (int i = 0; i < 32 && BitOperations.PopCount(mask) < amount; i++)
                    if ((objective.Progress & (1u << i)) == 0u)
                        mask |= 1u << i;
                amount = mask;
            }

            player.QuestManager.ObjectiveUpdate(objective.ObjectiveInfo.Id, amount);
            return SpellEffectExecutionResult.Ok;
        }
    }
}
