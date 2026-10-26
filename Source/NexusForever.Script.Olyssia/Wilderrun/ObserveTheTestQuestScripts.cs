using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Olyssia.Wilderrun
{
    /// <summary>
    /// "Observe the Test" of the Molecular Reconstructor in Wilderrun: a research jabbit is put through the machine and comes out twisted.
    /// </summary>
    /// <remarks>
    /// The creatures are the quests' own, tagged in their descriptions: the Research Jabbit (Q7754 50839, Q8077 50670) and what it turns
    /// into, the Twisted Hunter (Q7754 39349) and Twisted Seeker (Q8077 40254), which the player deals with. Timing and that the twisted
    /// creature attacks are guesses.
    /// </remarks>
    public abstract class ObserveTheTestQuestScript : QuestSceneScript
    {
        protected abstract uint ObserveObjectiveId { get; }
        protected abstract uint ResearchJabbit { get; }
        protected abstract uint TwistedCreature { get; }
        protected abstract Vector3 TestPosition { get; }

        protected override IReadOnlyList<uint> SceneObjectiveIds => [ObserveObjectiveId];

        #region Dependency Injection

        private readonly ICreatureInfoManager creatureInfoManager;

        protected ObserveTheTestQuestScript(ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        protected override void OnSceneStart(uint objectiveId, IPlayer player)
        {
            ICreatureInfo jabbit = creatureInfoManager.GetCreatureInfo(ResearchJabbit);
            ICreatureInfo twisted = creatureInfoManager.GetCreatureInfo(TwistedCreature);
            if (jabbit == null || twisted == null)
                return;

            player.SummonFactory.Summon<INonPlayerEntity>(jabbit, TestPosition, Vector3.Zero, (map, guid, vector) =>
            {
                // the jabbit goes through the machine and comes out twisted
                After(5d, () =>
                {
                    map.GetEntity<IWorldEntity>(guid)?.RemoveFromMap();
                    player.SummonFactory.Summon<INonPlayerEntity>(twisted, TestPosition, Vector3.Zero, (m, g, v) =>
                    {
                        IUnitEntity summoned = m.GetEntity<IUnitEntity>(g);
                        summoned?.ThreatManager.UpdateThreat(player, 1);
                        After(90d, () => m.GetEntity<IWorldEntity>(g)?.RemoveFromMap());
                    });
                });
            });
        }
    }

    /// <summary>
    /// Symptoms May Include (Exile).
    /// </summary>
    [ScriptFilterOwnerId(7754)]
    public class SymptomsMayIncludeQuestScript : ObserveTheTestQuestScript
    {
        protected override uint ObserveObjectiveId => 18060u;
        protected override uint ResearchJabbit => 50839u;
        protected override uint TwistedCreature => 39349u;
        // the objective's marker, WorldLocation2 45035
        protected override Vector3 TestPosition => new(2607.7f, -776.1f, -4284.3f);

        public SymptomsMayIncludeQuestScript(ICreatureInfoManager creatureInfoManager)
            : base(creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Reckless Research (Dominion).
    /// </summary>
    [ScriptFilterOwnerId(8077)]
    public class RecklessResearchQuestScript : ObserveTheTestQuestScript
    {
        protected override uint ObserveObjectiveId => 18008u;
        protected override uint ResearchJabbit => 50670u;
        protected override uint TwistedCreature => 40254u;
        // the objective's marker, WorldLocation2 44888
        protected override Vector3 TestPosition => new(2385.4f, -789f, -3994.9f);

        public RecklessResearchQuestScript(ICreatureInfoManager creatureInfoManager)
            : base(creatureInfoManager)
        {
        }
    }
}
