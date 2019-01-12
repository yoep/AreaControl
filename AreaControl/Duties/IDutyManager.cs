using Rage;

namespace AreaControl.Duties
{
    public interface IDutyManager
    {
        /// <summary>
        /// Get the next available duty in the area if found.
        /// Otherwise, get the idle duty.
        /// </summary>
        /// <param name="position">Set the position to search in for an available duty.</param>
        /// <returns>Returns the next available duty if available, else null.</returns>
        IDuty NextAvailableOrIdleDuty(Vector3 position);

        /// <summary>
        /// Register the duty in the <see cref="IDutyManager"/>.
        /// This method must be invoked for each created duty outside of the <see cref="IDutyManager"/>.
        /// </summary>
        /// <param name="duty">Set the duty to register.</param>
        void RegisterDuty(IDuty duty);

        /// <summary>
        /// Abort all active duties and clean them from the duty manager so they can be instantiated again.
        /// </summary>
        void DismissDuties();
    }
}