using System.Numerics;
using NexusForever.Database.Character;
using NexusForever.Game.Static.PlayerPath;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Abstract.Entity
{
    public interface IPathMissionManager : IDatabaseCharacter, IEnumerable<IPathMission>
    {
        /// <summary>
        /// Current <see cref="PathEpisodeEntry"/> for the zone the player is in, for the active path.
        /// </summary>
        PathEpisodeEntry CurrentEpisode { get; }

        void SendInitialPackets();

        /// <summary>
        /// Invoked when the player enters a new zone.
        /// </summary>
        void OnZoneUpdate();

        /// <summary>
        /// Invoked when the player moves.
        /// </summary>
        void OnRelocate(Vector3 position);

        /// <summary>
        /// Invoked when the player changes their active path.
        /// </summary>
        void OnPathChanged();

        IPathMission GetMission(uint missionId);

        /// <summary>
        /// Return the mission, unlocking it first if the player meets its requirements, e.g. when the player uses the mission's object directly.
        /// </summary>
        IPathMission GetOrUnlockMission(uint missionId);

        /// <summary>
        /// Unlock the supplied mission for the player, optionally playing the communicator unlock message.
        /// </summary>
        IPathMission UnlockMission(PathMissionEntry entry, bool notify = true);

        /// <summary>
        /// Set the progress and state flags of an unlocked mission, completing it if all objectives are met.
        /// </summary>
        void UpdateMission(IPathMission mission, uint progress, uint state);

        /// <summary>
        /// Complete an unlocked mission, granting path experience and rewards.
        /// </summary>
        void CompleteMission(IPathMission mission);

        /// <summary>
        /// Invoked when the client reports reaching an explorer node for a mission.
        /// </summary>
        void OnExplorerProgressReport(uint missionId, uint nodeIndex);

        /// <summary>
        /// Invoked when a zone map hex group is discovered.
        /// </summary>
        void OnZoneMapUpdate(uint mapZoneId, bool complete);

        /// <summary>
        /// Invoked when the player gets kill credit for an entity.
        /// </summary>
        void OnCreatureKilled(IWorldEntity entity);

        /// <summary>
        /// Invoked when the player successfully activates an entity.
        /// </summary>
        void OnEntityActivated(IWorldEntity entity);

        /// <summary>
        /// Invoked when the player completes a quest.
        /// </summary>
        void OnQuestCompleted(uint questId);

        /// <summary>
        /// Invoked when the player discovers a datacube.
        /// </summary>
        void OnDatacubeDiscovered(uint datacubeId);

        /// <summary>
        /// Invoked when the player interacts with an entity, gives missions with an unlock creature and starts scavenger hunts.
        /// </summary>
        void OnEntityInteract(IWorldEntity entity);

        /// <summary>
        /// Invoked when the player uses the explorer path action to search for a scavenger hunt dig location.
        /// </summary>
        void OnExplorerSearch(uint clueId, byte searchRadiusBand);

        /// <summary>
        /// Returns if the player has completed the supplied mission.
        /// </summary>
        bool IsMissionComplete(uint missionId);
    }
}
