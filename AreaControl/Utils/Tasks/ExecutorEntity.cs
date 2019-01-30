using Rage;

namespace AreaControl.Utils.Tasks
{
    public class ExecutorEntity
    {
        public const int UnknownCompletionStatus = 404;

        public ExecutorEntity(Ped ped)
        {
            Ped = ped;
            CompletionStatus = UnknownCompletionStatus;
        }

        /// <summary>
        /// Get the ped instance of this executor entity.
        /// </summary>
        public Ped Ped { get; }

        /// <summary>
        /// Get if the task for the ped has been completed.
        /// </summary>
        public bool CompletedTask { get; internal set; }

        /// <summary>
        /// Get the completion status code if applicable.
        /// If -99 = still unknown status
        /// </summary>
        public int CompletionStatus { get; internal set; }

        public override string ToString()
        {
            return
                $"{nameof(Ped)}: {Ped}, {nameof(CompletedTask)}: {CompletedTask}, {nameof(CompletionStatus)}: {CompletionStatus}";
        }
    }
}