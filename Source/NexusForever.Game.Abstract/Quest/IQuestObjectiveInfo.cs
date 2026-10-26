using NexusForever.Game.Static.Quest;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Abstract.Quest
{
    public interface IQuestObjectiveInfo
    {
        uint Id { get; }
        QuestObjectiveType Type { get; }

        QuestObjectiveEntry Entry { get; }

        /// <summary>
        /// Quest objective is sequential, previous quest objective must be complete.
        /// </summary>
        bool IsSequential();

        /// <summary>
        /// Quest objective is hidden until previous objective is complete.
        /// </summary>
        bool IsHidden();

        /// <summary>
        /// Quest objective is optional, doesn't need to be complete for quest to be achieved.
        /// </summary>
        bool IsOptional();

        bool HasUnknown0200();

        /// <summary>
        /// Return the world locations an enter area objective is completed at.
        /// </summary>
        IEnumerable<WorldLocation2Entry> GetAreaLocations();

        /// <summary>
        /// Returns if the objective is completed by reaching its area, enter area objectives and target-less objectives asking to go somewhere.
        /// </summary>
        bool IsAreaObjective();

        /// <summary>
        /// Return the creature a target-less "Listen to" objective names, 0 if it isn't one.
        /// </summary>
        uint GetListenCreatureId();

        /// <summary>
        /// Return the creature a target-less "Use/Open/Select the" objective names, 0 if it isn't one.
        /// </summary>
        uint GetUseCreatureId();

        /// <summary>
        /// Return the creature a target-less "Follow" objective names, 0 if it isn't one.
        /// </summary>
        uint GetFollowCreatureId();

        /// <summary>
        /// Return what a target-less activate objective asks for when its text names it, <see cref="QuestTextTrigger.None"/> if it isn't one.
        /// </summary>
        (QuestTextTrigger Kind, uint Id, string Name) GetTextTrigger();

        /// <summary>
        /// Return the creature and emote command of an objective asking for an emote before a creature, (0, null) if it isn't one.
        /// </summary>
        (uint CreatureId, string Command) GetEmoteTrigger();

        /// <summary>
        /// Returns if the objective has no target of its own and is advanced by using something at its marker.
        /// </summary>
        bool IsActivationAtMarker();
    }
}