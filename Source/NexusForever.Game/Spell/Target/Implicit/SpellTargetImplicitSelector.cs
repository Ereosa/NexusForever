using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Search;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Info;
using NexusForever.Game.Abstract.Spell.Target.Implicit;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell.Target.Implicit
{
    public class SpellTargetImplicitSelector : ISpellTargetImplicitSelector
    {
        private IUnitEntity caster;
        private Vector3 initialPosition;
        private ISpellInfo info;

        #region Dependency Injection

        private readonly ISearchCheckSpellTargetImplicit searchCheck;

        public SpellTargetImplicitSelector(
            ISearchCheckSpellTargetImplicit searchCheck)
        {
            this.searchCheck = searchCheck;
        }

        #endregion

        public void Initialise(IUnitEntity caster, ISpellParameters parameters)
        {
            if (this.caster != null)
                throw new InvalidOperationException();

            this.caster = caster;
            info = parameters.SpellInfo;

            initialPosition = caster.Position;
            if (parameters.PositionalUnitId > 0)
            {
                IWorldEntity positionalUnit = caster.GetVisible<IWorldEntity>(parameters.PositionalUnitId);
                if (positionalUnit == null)
                    throw new InvalidOperationException();

                initialPosition = positionalUnit.Position;
            }
            else if (parameters.PrimaryTargetId > 0
                && info.BaseInfo.TargetMechanics?.TargetType is SpellTargetMechanicType.PrimaryTargetArea or SpellTargetMechanicType.PrimaryTargetPosition)
            {
                // aoe centered on the primary target, eg: 34589 Artillerybot Barrage hitting foes around the barrage target
                IUnitEntity primaryTarget = caster.GetVisible<IUnitEntity>(parameters.PrimaryTargetId);
                if (primaryTarget != null)
                    initialPosition = primaryTarget.Position;
            }

            searchCheck.Initialise(caster, initialPosition, info.Entry.TargetMaxRange, info.BaseInfo.TargetMechanics.Flags);
        }

        public void SelectTargets(List<ISpellTargetImplicit> implicitTargets)
        {
            // TODO: Use Target Type to calculate positions
            foreach (IUnitEntity target in caster.Map.Search(initialPosition, info.Entry.TargetMaxRange, searchCheck))
                implicitTargets.Add(new SpellTargetImplicit(target, Vector3.Distance(initialPosition, target.Position)));
        }
    }
}
