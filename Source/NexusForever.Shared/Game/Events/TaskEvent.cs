using System;
using System.Threading.Tasks;
using NLog;

namespace NexusForever.Shared.Game.Events
{
    /// <summary>
    /// An <see cref="IEvent"/> that will execute an <see cref="Action"/> after <see cref="Task"/> completion.
    /// </summary>
    public class TaskEvent : IEvent
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private readonly Task task;
        private readonly Action callback;

        public TaskEvent(Task task, Action callback)
        {
            this.task     = task;
            this.callback = callback;
        }

        /// <summary>
        /// Returns if <see cref="TaskEvent"/> can be executed.
        /// </summary>
        public bool CanExecute()
        {
            return task.IsCompleted;
        }

        /// <summary>
        /// Executes <see cref="TaskEvent"/> action.
        /// </summary>
        public void Execute()
        {
            // a failed task (eg: a character save) used to vanish silently, losing everything the save contained
            if (task.IsFaulted)
                log.Error(task.Exception, "Task event failed!");

            callback?.Invoke();
        }
    }
}
