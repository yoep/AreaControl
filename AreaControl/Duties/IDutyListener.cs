using System;
using System.Collections.Generic;
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
        /// Get the list of duty types to check the availability of.
        /// Additional duty types can be added to this list.
        /// </summary>
        IList<DutyType> DutyTypes { get; }

        /// <summary>
        /// Register an event that will be triggered when a new duty is available.
        /// </summary>
        EventHandler<DutyAvailableEventArgs> OnDutyAvailable { get; set; }
    }
}