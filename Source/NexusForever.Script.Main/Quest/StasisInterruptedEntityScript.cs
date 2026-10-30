using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Quest
{
    /// <summary>
    /// Stasis, Interrupted (Crimson Isle, 8855 and 8856): "Jump through stasis fields to free Dominion Soldiers near the Megatech Shield Generator".
    /// </summary>
    /// <remarks>
    /// The objective has no data, its reward pane target group 6671 lists the Captured Dominion Soldiers (47687, 47688). Each soldier hangs
    /// about 5 m above its Exile Stasis Field (47684), which sits on the ground under it, so the player passes through the column between the
    /// field and the soldier rather than the soldier itself. A player inside that column (within 3 m horizontally, from about 1.5 m below the field to
    /// 2 m above the soldier, guessed from the field's size) frees the soldier. Each soldier only counts once per character.
    /// </remarks>
    [ScriptFilterCreatureId(47687, 47688)]
    public class StasisInterruptedEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const ushort StasisInterruptedA = 8855;
        private const ushort StasisInterruptedB = 8856;
        private const uint CapturedSoldierTargetGroup = 6671;
        private const float StasisFieldRadius = 3f;
        private const float BelowField = 6.5f;
        private const float AboveSoldier = 2f;
        private const float WatchRange = 10f;

        private IWorldEntity owner;
        private readonly HashSet<ulong> freedFor = [];
        private readonly Dictionary<uint, IPlayer> nearby = [];
        private double check;

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
            owner.SetInRangeCheck(WatchRange);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to range check range.
        /// </summary>
        public void OnEnterRange(IGridEntity entity)
        {
            if (entity is IPlayer player)
                nearby[player.Guid] = player;
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is removed from range check range.
        /// </summary>
        public void OnExitRange(IGridEntity entity)
        {
            nearby.Remove(entity.Guid);
        }

        public void Update(double lastTick)
        {
            if (nearby.Count == 0)
                return;

            check -= lastTick;
            if (check > 0d)
                return;
            check = 0.1d;

            foreach (IPlayer player in nearby.Values.ToList())
            {
                if (player.Map != owner.Map)
                {
                    nearby.Remove(player.Guid);
                    continue;
                }

                if (!InStasisField(player.Position) || freedFor.Contains(player.CharacterId))
                    continue;

                if (player.QuestManager.GetActiveQuest(StasisInterruptedA) == null && player.QuestManager.GetActiveQuest(StasisInterruptedB) == null)
                    continue;

                freedFor.Add(player.CharacterId);
                player.QuestManager.ObjectiveUpdateRewardPane(QuestObjectiveType.ActivateEntity, CapturedSoldierTargetGroup, 1u);
            }
        }

        private bool InStasisField(Vector3 position)
        {
            float dx = position.X - owner.Position.X;
            float dz = position.Z - owner.Position.Z;
            if (dx * dx + dz * dz > StasisFieldRadius * StasisFieldRadius)
                return false;

            return position.Y >= owner.Position.Y - BelowField && position.Y <= owner.Position.Y + AboveSoldier;
        }
    }
}
