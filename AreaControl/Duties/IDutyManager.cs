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
        /// Get the next available duty in the area of the ped if found.
        /// </summary>
        /// <param name="ped">Set the ped to get the next available duty for.</param>
        /// <param name="dutyTypes">Set the list of duties to choose from.</param>
        /// <returns>Returns the next available duty if available, else null.</returns>
        IDuty NextAvailableDuty(ACPed ped, IEnumerable<DutyType> dutyTypes);

        /// <summary>
        /// Get the next available duty in the area if found.
        /// Otherwise, get the default idle duty (<see cref="ReturnToVehicleDuty"/>).
        /// The returned duty is automatically registered as a duty for the ped.
        /// </summary>
        /// <param name="ped">Set the ped to get the next available duty for.</param>
        /// <param name="dutyTypes">Set the list of duties to choose from.</param>
        /// <returns>Returns the next available duty if available, else <see cref="ReturnToVehicleDuty"/>.</returns>
        IDuty NextAvailableOrIdleDuty(ACPed ped, IEnumerable<DutyType> dutyTypes);

        /// <summary>
        /// Get a list of all registered duties.
        /// </summary>
        IReadOnlyList<IDuty> RegisteredDuties { get; }

        /// <summary>
        /// Get the next unique duty id.
        /// </summary>
        /// <returns>Returns the next duty id.</returns>
        long GetNextDutyId();

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