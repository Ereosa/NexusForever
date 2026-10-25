using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.PathXpModify)]
    public class SpellEffectPathXpModifyHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        #region Dependency Injection

        private readonly IGameTableManager gameTableManager;

        public SpellEffectPathXpModifyHandler(
            IGameTableManager gameTableManager)
        {
            this.gameTableManager = gameTableManager;
        }

        #endregion

        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        /// <remarks>
        /// DataBits01 1 grants path levels (e.g. "Path Level Up" items), otherwise DataBits00 is a number of path mission rewards (e.g. discoveries).
        /// </remarks>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (target is not IPlayer player || data.DataBits00 == 0u)
                return SpellEffectExecutionResult.PreventEffect;

            uint xp;
            if (data.DataBits01 == 1u)
            {
                uint level       = player.PathManager.GetCurrentLevel(player.Path);
                uint targetLevel = level + data.DataBits00;

                PathLevelEntry current = gameTableManager.PathLevel.Entries
                    .FirstOrDefault(e => e != null && e.PathTypeEnum == (uint)player.Path && e.PathLevel == level);
                PathLevelEntry next = gameTableManager.PathLevel.Entries
                    .Where(e => e != null && e.PathTypeEnum == (uint)player.Path && e.PathLevel <= targetLevel)
                    .MaxBy(e => e.PathLevel);
                if (current == null || next == null || next.PathXP <= current.PathXP)
                    return SpellEffectExecutionResult.PreventEffect;

                xp = next.PathXP - current.PathXP;
            }
            else
            {
                // client PathMission:GetRewardXp
                uint missionXp = gameTableManager.GameFormula.GetEntry(378)?.Dataint0 ?? 50u;
                xp = missionXp * data.DataBits00;
            }

            player.PathManager.AddXp(xp);
            return SpellEffectExecutionResult.Ok;
        }
    }
}
