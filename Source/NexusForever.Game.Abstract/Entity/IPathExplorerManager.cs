namespace NexusForever.Game.Abstract.Entity
{
    public interface IPathExplorerManager
    {
        void Update(double lastTick);

        /// <summary>
        /// Invoked when the client uses the explorer path action while following a power map traveler.
        /// </summary>
        void OnPowerMapProgress(uint missionId);

        /// <summary>
        /// Invoked when the player activates an entity, explorer switches raise their hidden path platforms.
        /// </summary>
        void OnEntityActivated(IWorldEntity entity);

        /// <summary>
        /// Cancel any running power map, e.g. on map change or path change.
        /// </summary>
        void Cancel();
    }
}
