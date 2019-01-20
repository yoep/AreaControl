using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AreaControl.Instances;

namespace AreaControl.Duties
{
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    public interface IDutyManager
    {
        /// <summary>
        /// Listen for a new available duty around the given position.
        /// Will return a new listener that will be triggered when a new duty becomes available around the given position.
        /// </summary>
        /// <param name="ped">Set the ped to listen for new duties.</param>
        IDutyListener this[ACPed ped] { get; }

        /// <summary>
        /// Get the next available duty in the area if found.
        /// Otherwise, get the idle duty.
        /// The returned duty is automatically registered as a duty for the ped.
        /// </summary>
        /// <param name="ped">Set the ped to get the next available duty for.</param>
        /// <returns>Returns the next available duty if available, else null.</returns>
        IDuty NextAvailableOrIdleDuty(ACPed ped);

        /// <summary>
        /// Get a list of all registered duties.
        /// </summary>
        IReadOnlyList<IDuty> RegisteredDuties { get; }

        /// <summary>
        /// Register the duty in the <see cref="IDutyManager"/>.
        /// This method must be invoked for each created duty outside of the <see cref="IDutyManager"/>.
        /// </summary>
        /// <param name="ped">Set the ped the duty is being registered for.</param>
        /// <param name="duty">Set the duty to register.</param>
        void RegisterDuty(ACPed ped, IDuty duty);

        /// <summary>
        /// Abort all active duties and clean them from the duty manager so they can be instantiated again.
        /// </summary>
        void DismissDuties();
    }
}