using NexusForever.Database.World.Model;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Entity.Movement;
using NexusForever.Game.Static.Entity;
using NexusForever.Network.World.Entity;
using NexusForever.Network.World.Entity.Model;
using NexusForever.Network.World.Message.Model;
using NexusForever.Script;
using NexusForever.Shared.Game.Events;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Collection;

namespace NexusForever.Game.Entity
{
    public class DoorEntity : WorldEntity, IDoorEntity
    {
        public override EntityType Type => EntityType.Door;

        public bool IsOpen => GetStatEnum<StandState>(Static.Entity.Stat.StandState) == StandState.State1;

        #region Dependency Injection

        private readonly IScriptManager scriptManager;

        public DoorEntity(
            IScriptManager scriptManager,
            IMovementManager movementManager,
            IEntitySummonFactory entitySummonFactory)
            : base(movementManager, entitySummonFactory)
        {
            this.scriptManager = scriptManager;
        }

        #endregion

        public override void Initialise(ICreatureInfo creatureInfo, EntityModel model)
        {
            base.Initialise(creatureInfo, model);

            SetStandState(StandState.State0); // Closed on spawn
            SetBaseProperty(Property.BaseHealth, 101f); // Sniffs showed all doors had 101hp for me.
        }

        /// <summary>
        /// Initialise <see cref="IScriptCollection"/> for <see cref="IDoorEntity"/>.
        /// </summary>
        protected override void InitialiseScriptCollection(List<string> names)
        {
            scriptCollection = scriptManager.InitialiseOwnedCollection<IDoorEntity>(this);
            scriptManager.InitialiseEntityScripts<IDoorEntity>(scriptCollection, this, names);
        }

        protected override IEntityModel BuildEntityModel()
        {
            return new DoorEntityModel
            {
                CreatureId = CreatureId
            };
        }

        /// <summary>
        /// Used to open this <see cref="IDoorEntity"/>.
        /// </summary>
        public void OpenDoor()
        {
            SetStandState(StandState.State1);

            // doors only showed open to clients that created them already open: the stand state change alone left Fragment Zero's
            // inner airlock irises and Outpost M-13's bridge door shut for the players next to them, and a destroy and create in
            // the same tick changed nothing (the client still had the entity). Destroy it for everyone who can see it and create
            // it again, open, a second later.
            if (Map != null)
            {
                EnqueueToVisible(new ServerEntityDestroy
                {
                    Guid     = Guid,
                    Unknown0 = true
                });
                eventQueue.EnqueueEvent(new DelayEvent(TimeSpan.FromSeconds(1d), () =>
                {
                    if (Map != null)
                        EnqueueToVisible(BuildCreatePacket(false));
                }));
            }

            scriptCollection.Invoke<IDoorEntityScript>(script => script.OnOpenDoor());
        }

        /// <summary>
        /// Used to close this <see cref="IDoorEntity"/>.
        /// </summary>
        public void CloseDoor()
        {
            SetStandState(StandState.State0);

            scriptCollection.Invoke<IDoorEntityScript>(script => script.OnDoorClose());
        }
    }
}
