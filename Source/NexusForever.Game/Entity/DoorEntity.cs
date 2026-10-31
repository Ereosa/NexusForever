using NexusForever.Database.World.Model;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Entity.Movement;
using NexusForever.Game.Static.Entity;
using NexusForever.Network.World.Entity;
using NexusForever.Network.World.Entity.Model;
using NexusForever.Network.World.Message.Model;
using NexusForever.Script;
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
        /// How long a door opened by a player stays open (guessed).
        /// </summary>
        private const double UsedDoorOpenTime = 20d;

        private double closeTimer;

        /// <summary>
        /// A door with an activate spell (eg: the Cage Door of Information Extraction, report 138) opens when a player uses it, the
        /// activate effect never reaches it (doors aren't units), the interaction succeeds here.
        /// </summary>
        public override void OnActivateSuccess(IPlayer activator)
        {
            base.OnActivateSuccess(activator);

            if (InWorld && !IsOpen)
            {
                OpenDoor();
                closeTimer = UsedDoorOpenTime;
            }
        }

        public override void Update(double lastTick)
        {
            base.Update(lastTick);

            if (closeTimer <= 0d)
                return;

            closeTimer -= lastTick;
            if (closeTimer <= 0d && IsOpen)
                CloseDoor();
        }

        /// <summary>
        /// Used to open this <see cref="IDoorEntity"/>.
        /// </summary>
        public void OpenDoor()
        {
            SetStandState(StandState.State1);

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
