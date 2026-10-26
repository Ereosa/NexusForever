using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Script.Olyssia.Deradune.Event;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Olyssia.Deradune.Script
{
    /// <summary>
    /// Metal Maw's cage in the Bloodfire arena, runs the Metal Maw world boss event (<see cref="MetalMawEventScript"/>).
    /// </summary>
    /// <remarks>
    /// Players near the arena take part in the event. When Metal Maw breaks out his intro (GC016) plays for the players around
    /// (once per character until the server restarts) and the boss is spawned on the cage. After a kill the event comes back
    /// once the one hour reset of the Progenitor Data Cache (objective 4992) is over. Range and spawn spot are guessed.
    /// </remarks>
    [ScriptFilterCreatureId(26018)]
    public class MetalMawCageEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint MetalMawCreatureId = 24740;
        private const float EventRange = 120f;
        private static readonly TimeSpan ResetTime = TimeSpan.FromHours(1);

        private static readonly HashSet<ulong> introPlayed = [];

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;
        private readonly ICreatureInfoManager creatureInfoManager;

        public MetalMawCageEntityScript(
            ICinematicFactory cinematicFactory,
            ICreatureInfoManager creatureInfoManager)
        {
            this.cinematicFactory    = cinematicFactory;
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        private IWorldEntity owner;
        private IPublicEvent publicEvent;
        private uint bossGuid;
        private double resetTimer;

        private readonly HashSet<uint> playersInRange = [];
        private readonly HashSet<ulong> participants = [];

        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
            owner.SetInRangeCheck(EventRange);
        }

        public void OnEnterRange(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return;

            playersInRange.Add(player.Guid);
            Join(player);
        }

        public void OnExitRange(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return;

            playersInRange.Remove(player.Guid);
            if (participants.Remove(player.CharacterId) && publicEvent is { HasFinished: false })
                publicEvent.LeaveEvent(player, PublicEventRemoveReason.LeftArea);
        }

        public void Update(double lastTick)
        {
            if (owner?.Map == null)
                return;

            if (publicEvent != null && publicEvent.HasFinished)
                OnEventFinished();

            if (publicEvent == null)
            {
                if (resetTimer > 0d)
                {
                    resetTimer -= lastTick;
                    return;
                }

                if (playersInRange.Count > 0)
                    StartEvent();
                return;
            }

            if ((MetalMawEventScript.Phase)publicEvent.Phase == MetalMawEventScript.Phase.Fight && bossGuid == 0u)
                BreakOut();
        }

        private void StartEvent()
        {
            // an event that finished without a winner stays in the store until it is finalised
            if (owner.Map.PublicEventManager.GetEvent(MetalMawEventScript.EventId) != null)
                return;

            publicEvent = owner.Map.PublicEventManager.CreateEvent(MetalMawEventScript.EventId);
            if (publicEvent == null)
                return;

            participants.Clear();
            foreach (uint guid in playersInRange)
                if (owner.GetVisible<IPlayer>(guid) is IPlayer player)
                    Join(player);
        }

        private void Join(IPlayer player)
        {
            if (publicEvent == null || publicEvent.HasFinished || !participants.Add(player.CharacterId))
                return;

            publicEvent.JoinEvent(player, PublicEventTeam.PublicTeam);
        }

        private void OnEventFinished()
        {
            IUnitEntity boss = bossGuid != 0u ? owner.GetVisible<IUnitEntity>(bossGuid) : null;
            // finished without a kill when the last player left, it comes back as soon as someone returns
            bool killed = bossGuid != 0u && boss is not { IsAlive: true };

            if (boss is { IsAlive: true })
                boss.RemoveFromMap();

            bossGuid    = 0u;
            publicEvent = null;
            participants.Clear();
            resetTimer  = killed ? ResetTime.TotalSeconds : 0d;
        }

        private void BreakOut()
        {
            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(MetalMawCreatureId);
            if (creatureInfo == null)
                return;

            IWorldEntity boss = owner.SummonFactory.Summon(creatureInfo, EntityType.NonPlayer, owner.Position, owner.Rotation);
            if (boss == null)
                return;

            bossGuid = boss.Guid;

            foreach (uint guid in playersInRange)
            {
                if (owner.GetVisible<IPlayer>(guid) is not IPlayer player || player.CinematicManager.IsPlaying)
                    continue;

                if (!introPlayed.Add(player.CharacterId))
                    continue;

                ICinematicBase cinematic = cinematicFactory.CreateCinematic(CinematicSceneId.DeraduneMetalMawIntro);
                if (cinematic != null)
                    player.CinematicManager.QueueCinematic(cinematic);
            }
        }
    }
}
