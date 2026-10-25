namespace NexusForever.Game.Abstract.Entity
{
    public interface IPathSettlerManager
    {
        /// <summary>
        /// Invoked when the player interacts with a settler depot, opens the build window for its improvements.
        /// </summary>
        void OpenDepot(IWorldEntity depot);

        /// <summary>
        /// Build or contribute to the supplied tier of a PathSettlerImprovementGroup.
        /// </summary>
        void BuildImprovement(uint improvementGroupId, uint tier);

        /// <summary>
        /// Invoked when the player enters a new zone, sends the status of the settler hub for the zone.
        /// </summary>
        void OnZoneUpdate();
    }
}
