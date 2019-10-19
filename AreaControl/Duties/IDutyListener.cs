using System;
using AreaControl.Duties.Flags;
using AreaControl.Instances;

namespace AreaControl.Duties
{
    public interface IDutyListener
    {
        /// <summary>
        /// Get the ped the duty listener is listening for.
        /// </summary>
        ACPed Ped { get; }
        
        /// <summary>
        /// Get or set the duty types to check the availability of.
        /// If <see cref="DutyTypeFlag.None"/>, the list of duties is fetched based on the <see cref="PedType"/> of the ped.
        /// </summary>
        DutyTypeFlag DutyTypes { get; set; }

        /// <summary>
        /// Register an event that will be triggered when a new duty is available (optional).
        /// </summary>
        EventHandler<DutyAvailableEventArgs> OnDutyAvailable { get; set; }
    }
}