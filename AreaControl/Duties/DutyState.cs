namespace AreaControl.Duties
{
    public enum DutyState
    {
        /// <summary>
        /// Duty is being initialized by the <see cref="IDutyManager"/>.
        /// </summary>
        Initializing,
        /// <summary>
        /// Duty is ready to be executed.
        /// </summary>
        Ready,
        /// <summary>
        /// Duty is currently active.
        /// </summary>
        Active,
        /// <summary>
        /// Duty has been interrupted while it was active.
        /// This means that the duty can be resumed later on and continue with the execution.
        /// </summary>
        Interrupted,
        /// <summary>
        /// Duty has been completed with success.
        /// </summary>
        Completed,
        /// <summary>
        /// Duty has been aborted by an external event (such as <see cref="IDutyManager.DismissDuties"/>).
        /// </summary>
        Aborted
    }
}