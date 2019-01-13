using System;
using AreaControl.Instances;

namespace AreaControl.Duties
{
    public interface IDuty
    {
        /// <summary>
        /// Get if this duty is available to be executed.
        /// Some duties are always available and can be executed, while others need a certain condition to be present within their area
        /// (e.g. death bodies or wrecks in the area).
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Get if this duty is still active.
        /// Some duties can be completed and will become inactive when completed.
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Check if this duty can be repeated multiple times.
        /// Indicates if the duty can be executed multiple times if it has already been completed before.
        /// </summary>
        bool IsRepeatable { get; }
        
        /// <summary>
        /// Check if multiple instances are allowed at the same time of this duty.
        /// </summary>
        bool IsMultipleInstancesAllowed { get; }
        
        /// <summary>
        /// Add an event handler to the on completion event.
        /// </summary>
        EventHandler OnCompletion { get; set; }

        /// <summary>
        /// Execute this duty on the given ped.
        /// </summary>
        /// <param name="ped">Set the ped instance.</param>
        void Execute(ACPed ped);

        /// <summary>
        /// End forcefully the duty if it's still active.
        /// </summary>
        void Abort();
    }
}