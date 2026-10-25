namespace NexusForever.Game.Abstract.Entity
{
    public interface IPathScientistManager
    {
        /// <summary>
        /// Guid of the summoned scanbot, null if no scanbot is summoned.
        /// </summary>
        uint? ScanbotGuid { get; }

        void Update(double lastTick);

        /// <summary>
        /// Summon the scanbot for the supplied PathScientistScanBotProfile.
        /// </summary>
        void SummonScanbot(uint profileId);

        /// <summary>
        /// Dismiss the summoned scanbot, starting the scanbot cooldown.
        /// </summary>
        void DismissScanbot();

        /// <summary>
        /// Send the scanbot to scan the current target of the player.
        /// </summary>
        void ScanTarget();

        /// <summary>
        /// Invoked when an entity comes into vision range of the player, scannable entities are flagged for the client.
        /// </summary>
        void OnAddVisible(IWorldEntity entity);

        /// <summary>
        /// Invoked when a scientist mission is unlocked or completed, scannable flags are refreshed for visible entities.
        /// </summary>
        void RefreshScannableEntities();
    }
}
