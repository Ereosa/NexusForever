using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Entity.Trigger;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Game.Static.Reputation;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Adventure
{
    /// <summary>
    /// Adventures rebuilt from client data alone: an intro sub-event, then five stages along three storylines, the storyline of a stage chosen
    /// by the adventure's votes, each stage a sub-event whose objectives are driven from their client data.
    /// </summary>
    /// <remarks>
    /// The objectives that were scripted on retail are driven generically, all of a sub-event's main objectives at once: trigger volumes at
    /// their world locations, timed waits on their timers, kill and scripted objectives by the deaths of the sub-event's hostiles, target
    /// group objectives by the server (activations, kills). A sub-event finishes when its main objectives have, and a sub-event that makes
    /// no progress for eight minutes is finished for the players (something only retail scripting could complete).
    /// </remarks>
    public abstract class AdventureEventScript : ExpeditionEventScript, IPublicEventScript
    {
        protected abstract uint IntroEvent { get; }

        /// <summary>
        /// The three storylines' sub-events, stage one to five.
        /// </summary>
        protected abstract uint[][] Storylines { get; }

        /// <summary>
        /// The vote for a stage given the storylines chosen so far, with the storyline index of each option.
        /// </summary>
        protected abstract (uint VoteId, int[] Options)? GetVote(int stage, IReadOnlyList<int> chosen);

        /// <summary>
        /// Objectives of the adventure itself credited when the finale is done.
        /// </summary>
        protected abstract uint[] FinalObjectives { get; }

        protected override uint InitialPhase => 0u;
        protected override IReadOnlyDictionary<uint, uint> VeteranCreatures => new Dictionary<uint, uint>();

        private const double StallTime = 480d;

        private readonly List<int> chosen = [];
        private readonly List<IUnitEntity> hostiles = [];
        private readonly HashSet<uint> timed = [];
        private IPublicEvent current;
        private List<PublicEventObjectiveEntry> objectives = [];
        private int stage;
        private uint pendingVote;
        private int[] pendingOptions = [];
        private double stall;

        protected AdventureEventScript(
            IGlobalQuestManager globalQuestManager,
            ICreatureInfoManager creatureInfoManager,
            IEntityFactory entityFactory,
            IFactory<ISpellParameters> spellParametersFactory,
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
            : base(globalQuestManager, creatureInfoManager, entityFactory, spellParametersFactory, gameTableManager, cinematicFactory)
        {
        }

        protected override void OnPhase(uint phase)
        {
            if (phase != 0u)
                return;

            Delay(1d, () => StartSubEvent(IntroEvent));
        }

        public override void OnAddToMap(IGridEntity entity)
        {
            base.OnAddToMap(entity);

            if (entity is IPlayer player && current != null && !current.HasFinished)
                current.JoinEvent(player, PublicEventTeam.PublicTeam);

            if (current != null && entity is IUnitEntity unit and not IPlayer && unit.Faction1 != Faction.FriendlyToAll
                && unit.CreatureInfo?.Entry?.CreationTypeEnum == Game.Static.Entity.EntityType.NonPlayer)
                hostiles.Add(unit);
        }

        private void StartSubEvent(uint id)
        {
            hostiles.Clear();
            timed.Clear();
            stall = 0d;

            current = mapInstance.PublicEventManager.CreateEvent(id);
            foreach (IPlayer player in Players)
                current.JoinEvent(player, PublicEventTeam.PublicTeam);

            objectives = gameTableManager.PublicEventObjective.Entries
                .Where(o => o != null && o.PublicEventId == id && o.PublicEventObjectiveCategoryEnum == PublicEventObjectiveCategory.Main)
                .ToList();

            foreach (PublicEventObjectiveEntry objective in objectives)
                StartObjective(objective);

            OnSubEventStarted(id);
        }

        /// <summary>
        /// Invoked after a sub-event and its objectives started, for spawns the client data doesn't tie to the sub-event.
        /// </summary>
        protected virtual void OnSubEventStarted(uint id)
        {
        }

        private void StartObjective(PublicEventObjectiveEntry objective)
        {
            uint max = objective.PublicEventObjectiveTypeEnum is PublicEventObjectiveType.ParticipantsInTriggerVolume or PublicEventObjectiveType.TalkTo
                ? 0u : Math.Max(1u, objective.Count);
            current.ActivateObjective(objective.Id, max);

            switch (objective.PublicEventObjectiveTypeEnum)
            {
                case PublicEventObjectiveType.ParticipantsInTriggerVolume:
                {
                    WorldLocation2Entry location = gameTableManager.WorldLocation2.GetEntry(objective.WorldLocation2Id);
                    if (location == null)
                    {
                        Delay(5d, () => SubUpdate(objective.Id));
                        break;
                    }

                    var trigger = current.CreateEntity<IVolumeGridTriggerEntity>();
                    trigger.Initialise(objective.ObjectId, Math.Clamp(location.Radius, 4f, 30f), objective.ObjectId);
                    trigger.AddToMap(mapInstance, new Vector3(location.Position0, location.Position1, location.Position2));
                    break;
                }
                case PublicEventObjectiveType.TimedWin:
                {
                    double seconds = objective.FailureTimeMs > 0u ? objective.FailureTimeMs / 1000d : 20d;
                    IPublicEvent subEvent = current;
                    timed.Add(objective.Id);
                    Delay(seconds, () =>
                    {
                        if (subEvent == current)
                            SubUpdate(objective.Id, (int)Math.Max(1u, objective.Count));
                    });
                    break;
                }
            }
        }

        private void SubUpdate(uint objectiveId, int count = 1)
        {
            current?.UpdateObjective(objectiveId, count);
        }

        /// <summary>
        /// An objective of the running sub-event changed status.
        /// </summary>
        public void OnSubObjective(IPublicEvent subEvent, IPublicEventObjective objective)
        {
            if (subEvent != current || objective.Status != PublicEventStatus.Succeeded)
                return;

            stall = 0d;
            if (objectives.All(o => IsObjectiveDone(o.Id)))
                EndSubEvent();
        }

        private bool IsObjectiveDone(uint objectiveId)
        {
            IPublicEventTeam team = current?.GetTeams().FirstOrDefault(t => t.Team == PublicEventTeam.PublicTeam);
            return team != null && IsSucceeded(team, objectiveId);
        }

        private void EndSubEvent()
        {
            IPublicEvent finished = current;
            current = null;
            finished?.Finish(PublicEventTeam.PublicTeam);

            stage++;
            if (stage > 5)
            {
                foreach (uint objectiveId in FinalObjectives)
                    Update(objectiveId);
                CompleteExpedition([], [], 0, 0, 0, false);
                return;
            }

            (uint VoteId, int[] Options)? vote = GetVote(stage, chosen);
            if (vote.HasValue)
            {
                pendingVote = vote.Value.VoteId;
                pendingOptions = vote.Value.Options;
                publicEvent.StartVote(PublicEventTeam.PublicTeam, pendingVote, 0u);
                return;
            }

            Continue(Followed(stage == 5));
        }

        /// <summary>
        /// Stage three continues the latest choice, the finale is the storyline followed most (the latest choice breaks ties).
        /// </summary>
        private int Followed(bool most)
        {
            if (!most)
                return chosen[^1];

            return chosen
                .GroupBy(s => s)
                .OrderByDescending(g => g.Count())
                .ThenByDescending(g => chosen.LastIndexOf(g.Key))
                .First().Key;
        }

        public void OnVoteFinished(uint voteId, uint winner)
        {
            if (voteId != pendingVote)
                return;

            pendingVote = 0u;
            Continue(pendingOptions[Math.Min((int)winner, pendingOptions.Length - 1)]);
        }

        private void Continue(int storyline)
        {
            chosen.Add(storyline);
            StartSubEvent(Storylines[storyline][stage - 1]);
        }

        protected override void OnObjectiveSucceeded(uint objectiveId, IPublicEventTeam team)
        {
        }

        public override void OnEntityDeath(IUnitEntity entity)
        {
            if (current == null || !hostiles.Contains(entity))
                return;

            bool lastHostile = !hostiles.Any(u => u != entity && u.IsAlive && u.InWorld);
            foreach (PublicEventObjectiveEntry objective in objectives)
            {
                switch (objective.PublicEventObjectiveTypeEnum)
                {
                    case PublicEventObjectiveType.KillEventUnit:
                    case PublicEventObjectiveType.KillEventObjectiveUnit when objective.ObjectId == 0u:
                    case PublicEventObjectiveType.Script:
                        SubUpdate(objective.Id);
                        break;
                    case PublicEventObjectiveType.Exterminate when lastHostile:
                        SubUpdate(objective.Id);
                        break;
                }
            }
        }

        protected override void OnTick(double lastTick)
        {
            if (current == null || current.HasFinished)
                return;

            stall += lastTick;
            if (stall < StallTime)
                return;

            // nothing moved for a long time: finish what only retail scripting could
            stall = 0d;
            foreach (PublicEventObjectiveEntry objective in objectives.Where(o => !IsObjectiveDone(o.Id)).ToList())
                SubUpdate(objective.Id, (int)Math.Max(1u, objective.Count));
        }
    }
}
