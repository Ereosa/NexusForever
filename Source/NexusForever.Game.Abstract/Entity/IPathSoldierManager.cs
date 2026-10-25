namespace NexusForever.Game.Abstract.Entity
{
    public interface IPathSoldierManager
    {
        /// <summary>
        /// Returns if a holdout event is currently running for the player.
        /// </summary>
        bool InHoldout { get; }

        void Update(double lastTick);

        /// <summary>
        /// Invoked when the player activates an entity, soldier control points start their holdout.
        /// </summary>
        void OnActivate(IWorldEntity entity);

        /// <summary>
        /// Invoked when the player dies.
        /// </summary>
        void OnDeath();

        /// <summary>
        /// Cancel any running holdout without completing it, e.g. on logout or map change.
        /// </summary>
        void Cancel();
    }
}
