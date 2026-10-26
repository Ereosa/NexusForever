using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Adventure.LevianBay
{
    /// <summary>
    /// The Levian Bay adventure (public event 673, world 3176): Herald Anku'mar's speech on the shore, the Grand Evocator's three trials, then
    /// Calidor Antevian in Security Hub Sigma.
    /// </summary>
    /// <remarks>
    /// Built from the client data: The Truth Will Set You Free (672), the Trial of Will (674), the Trial of Cunning (675), the Charging
    /// Station (695) and the Trial of Endurance (676), each crediting the adventure's trial objectives, then meet and kill Calidor Antevian.
    /// Guessed: the Charging Station running right before the Trial of Endurance (its teleporter needs the charge), the trials in their
    /// objective order (the client offers no vote for them), the competition left to the sub-events' own objectives.
    /// </remarks>
    [ScriptFilterOwnerId(673)]
    public class LevianBayEventScript : AdventureEventScript
    {
        private const uint Calidor = 67701;

        private static readonly uint[] Order = [672, 674, 675, 695, 676];

        private static readonly Dictionary<uint, uint> TrialObjectives = new()
        {
            { 674, 4713 }, // Complete the Trial of Will
            { 675, 4714 }, // Complete the Trial of Cunning
            { 676, 4715 }  // Complete the Trial of Endurance
        };

        protected override uint WorldId => 3176;
        protected override uint IntroEvent => Order[0];
        protected override uint[][] Storylines => [];
        protected override uint[] FinalObjectives => [4358]; // Survive the Trials

        private bool finale;

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

        protected override void NextSubEvent(uint finishedId)
        {
            if (finishedId == Order[0])
            {
                Activate(4358);
                Activate(4713);
                Activate(4714);
                Activate(4715);
            }

            if (TrialObjectives.TryGetValue(finishedId, out uint trial))
                Update(trial);

            int next = Array.IndexOf(Order, finishedId) + 1;
            if (next > 0 && next < Order.Length)
            {
                StartSubEvent(Order[next]);
                return;
            }

            // the trials are survived: meet Calidor Antevian in Security Hub Sigma
            finale = true;
            Activate(4689);
            Delay(38d, () =>
            {
                Update(4689);
                Activate(4359);
                List<IUnitEntity> calidors = GetEntities<IUnitEntity>(Calidor).Where(c => c.IsAlive).ToList();
                foreach (IUnitEntity calidor in calidors)
                    AggroNearestPlayer(mapInstance, calidor.Guid, calidor.Position);

                // no sighting of him survived in the archive: the adventure ends with the trials
                if (calidors.Count == 0)
                {
                    Update(4359);
                    FinishAdventure();
                }
            });
        }

        public override void OnEntityDeath(IUnitEntity entity)
        {
            if (finale && entity.CreatureId == Calidor)
            {
                Update(4359);
                FinishAdventure();
                return;
            }

            base.OnEntityDeath(entity);
        }
    }
}
