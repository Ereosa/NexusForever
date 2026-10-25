using NexusForever.Game.Abstract.Entity.Creature;

namespace NexusForever.Game.Abstract.Entity
{
    public interface IScannerUnitEntity : IWorldEntity
    {
        uint OwnerGuid { get; }

        void Initialise(IPlayer owner, ICreatureInfo creatureInfo, string name);

        /// <summary>
        /// Move to the supplied entity, the scanner unit stops following its owner until <see cref="ReturnToOwner"/> is invoked.
        /// </summary>
        void MoveToTarget(IWorldEntity target);

        /// <summary>
        /// Resume following the owner.
        /// </summary>
        void ReturnToOwner();
    }
}
