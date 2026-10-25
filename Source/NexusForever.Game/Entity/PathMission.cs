using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.PlayerPath;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model.PlayerPath;

namespace NexusForever.Game.Entity
{
    public class PathMission : IPathMission
    {
        [Flags]
        public enum PathMissionSaveMask
        {
            None     = 0x00,
            Create   = 0x01,
            Progress = 0x02
        }

        public PathMissionEntry Entry { get; }
        public PathMissionType Type => (PathMissionType)Entry.PathMissionTypeEnum;
        public ushort Id => (ushort)Entry.Id;

        public bool Completed
        {
            get => completed;
            set
            {
                completed = value;
                saveMask |= PathMissionSaveMask.Progress;
            }
        }
        private bool completed;

        public uint Progress
        {
            get => progress;
            set
            {
                progress = value;
                saveMask |= PathMissionSaveMask.Progress;
            }
        }
        private uint progress;

        public uint State
        {
            get => state;
            set
            {
                state = value;
                saveMask |= PathMissionSaveMask.Progress;
            }
        }
        private uint state;

        public bool IsOptional => ((PathMissionFlags)Entry.PathMissionFlags & PathMissionFlags.Optional) != 0;

        private PathMissionSaveMask saveMask;

        private readonly IPlayer player;

        /// <summary>
        /// Create a new <see cref="IPathMission"/> from an existing database model.
        /// </summary>
        public PathMission(IPlayer player, PathMissionEntry entry, CharacterPathMissionModel model)
        {
            this.player = player;
            Entry       = entry;
            completed   = model.Completed != 0;
            progress    = model.Progress;
            state       = model.State;
        }

        /// <summary>
        /// Create a new unlocked <see cref="IPathMission"/>.
        /// </summary>
        public PathMission(IPlayer player, PathMissionEntry entry)
        {
            this.player = player;
            Entry       = entry;
            saveMask    = PathMissionSaveMask.Create;
        }

        public void Save(CharacterContext context)
        {
            if (saveMask == PathMissionSaveMask.None)
                return;

            var model = new CharacterPathMissionModel
            {
                Id        = player.CharacterId,
                MissionId = Id,
                Completed = Convert.ToByte(completed),
                Progress  = progress,
                State     = state
            };

            if ((saveMask & PathMissionSaveMask.Create) != 0)
                context.Add(model);
            else
            {
                EntityEntry<CharacterPathMissionModel> entity = context.Attach(model);
                entity.Property(p => p.Completed).IsModified = true;
                entity.Property(p => p.Progress).IsModified  = true;
                entity.Property(p => p.State).IsModified     = true;
            }

            saveMask = PathMissionSaveMask.None;
        }

        public Mission Build(PathMissionState missionState = PathMissionState.NoMission)
        {
            return new Mission
            {
                PathMissionId            = Id,
                Completed                = completed,
                ObjectiveCompletionFlags = progress,
                StateFlags               = state,
                State                    = missionState
            };
        }
    }
}
