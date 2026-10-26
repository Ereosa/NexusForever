using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Time tiered challenges whose goal is every one of their targets around the challenge indicator when the challenge starts.
    /// </summary>
    public abstract class ClearAreaChallengeScript : ChallengeScript
    {
        protected abstract uint[] TargetCreatureIds { get; }

        private uint goal;
        private uint done;

        public override void OnActivate()
        {
            done = 0u;

            WorldLocation2Entry area = GameTableManager.Instance.WorldLocation2.GetEntry(Challenge.Entry.WorldLocation2IdIndicator);
            goal = area == null ? 0u : (uint)TargetCreatureIds
                .SelectMany(c => Player.GetVisibleCreature<IWorldEntity>(c))
                .Count(e => HorizontalDistance(e.Position, new Vector3(area.Position0, area.Position1, area.Position2)) <= area.Radius);
        }

        /// <summary>
        /// Count one target done, completing the challenge once every target is.
        /// </summary>
        protected void CountTarget()
        {
            done++;
            AddProgress();
            if (Challenge.Activated && goal > 0u && done >= goal)
                Complete();
        }
    }

    /// <summary>
    /// Cellar Dwellers: "Rid the Hycrest cellars of Plagued Verminoid", every verminoid of the cellars before time runs out.
    /// </summary>
    [ScriptFilterOwnerId(271)]
    public class CellarDwellersChallengeScript : ClearAreaChallengeScript
    {
        protected override uint[] TargetCreatureIds { get; } = [24407, 24139, 24293];

        public override void OnEntityKilled(IUnitEntity entity)
        {
            if (TargetCreatureIds.Contains(entity.CreatureId))
                CountTarget();
        }
    }

    /// <summary>
    /// Veggie Delight: "Gather good Veggies before time runs out", the ch294 Sallec Radish, Sallec Carrot and Sallec Casca Carrot, the
    /// Plagued Sallec Radishes don't count. Every good vegetable around the farm before time runs out.
    /// </summary>
    [ScriptFilterOwnerId(294)]
    public class VeggieDelightChallengeScript : ClearAreaChallengeScript
    {
        protected override uint[] TargetCreatureIds { get; } = [25444, 25445, 25448];

        public override void OnEntityActivated(IWorldEntity entity)
        {
            if (TargetCreatureIds.Contains(entity.CreatureId))
                CountTarget();
        }
    }

    /// <summary>
    /// Shifting Priorities: "Reprogram Eldan relays before time runs out", each Eldan Relay used counts.
    /// </summary>
    [ScriptFilterOwnerId(437)]
    public class ShiftingPrioritiesChallengeScript : ChallengeScript
    {
        private const uint RelayCreatureId = 29893;

        public override void OnEntityActivated(IWorldEntity entity)
        {
            if (entity.CreatureId == RelayCreatureId)
                AddProgress();
        }
    }

    /// <summary>
    /// Battlesworn Medallion Collector: "Collect Battlesworn Medallions from Battlesworn Voreth during the siege of the Voreth Compound",
    /// each Battlesworn of the siege public event killed gives a medallion.
    /// </summary>
    [ScriptFilterOwnerId(492)]
    public class BattleswornMedallionCollectorChallengeScript : ChallengeScript
    {
        private static readonly uint[] battlesworn = [30347, 30549, 30558];

        public override void OnEntityKilled(IUnitEntity entity)
        {
            if (battlesworn.Contains(entity.CreatureId))
                AddProgress();
        }
    }

    /// <summary>
    /// Bringin' the Bingberries: "Gather bingberries with the aid of the Gravitic Destabilizers before time runs out", the Gravitic
    /// Destabilizers shake the Ripe Bingberries loose and they are gathered by running into them.
    /// </summary>
    /// <remarks>
    /// The number of bingberries to gather isn't in any data, the run is finished at <see cref="Goal"/>.
    /// </remarks>
    [ScriptFilterOwnerId(273)]
    public class BringinTheBingberriesChallengeScript : TouchChallengeScript
    {
        private const uint Goal = 10u;

        protected override uint[] CreatureIds { get; } = [25214];

        public override void Update(double lastTick)
        {
            base.Update(lastTick);
            if (Challenge.Activated && Challenge.Count >= Goal)
                Complete();
        }
    }
}
