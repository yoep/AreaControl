using System;
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
        /// Register an event that will be triggered when a new duty is available.
        /// </summary>
        EventHandler<DutyAvailableEventArgs> OnDutyAvailable { get; set; }
    }
}