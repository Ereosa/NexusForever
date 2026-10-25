using NexusForever.Database.Character;
using NexusForever.Game.Static.PlayerPath;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model.PlayerPath;

namespace NexusForever.Game.Abstract.Entity
{
    public interface IPathMission : IDatabaseCharacter
    {
        PathMissionEntry Entry { get; }
        PathMissionType Type { get; }
        ushort Id { get; }

        bool Completed { get; set; }

        /// <summary>
        /// Objective progress, depending on <see cref="PathMissionType"/> this is either a count/percentage or a bit mask of completed objectives.
        /// </summary>
        uint Progress { get; set; }

        /// <summary>
        /// Mission specific state flags.
        /// </summary>
        uint State { get; set; }

        bool IsOptional { get; }

        Mission Build(PathMissionState state = PathMissionState.NoMission);
    }
}
