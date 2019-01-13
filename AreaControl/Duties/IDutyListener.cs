using System;
using Rage;

namespace AreaControl.Duties
{
    public interface IDutyListener
    {
        /// <summary>
        /// Get the position the listener is searching on.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// Register an event that will be triggered when a new duty is available.
        /// </summary>
        EventHandler<DutyAvailableEventArgs> OnDutyAvailable { get; set; }
    }
}