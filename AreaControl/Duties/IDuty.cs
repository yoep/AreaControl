using System;
using AreaControl.Instances;

namespace AreaControl.Duties
{
    public interface IDuty
    {
        /// <summary>
        /// Get the unique id of this duty.
        /// </summary>
        long Id { get; }
        
        /// <summary>
        /// Get if this duty is available to be executed.
        /// Some duties are always available and can be executed, while others need a certain condition to be present within their area
        /// (e.g. death bodies or wrecks in the area).
        /// </summary>
        bool IsAvailable { get; }
        
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
        /// Get the state of the duty.
        /// </summary>
        DutyState State { get; }
        
        /// <summary>
        /// Add an event handler to the on completion event.
        /// </summary>
        EventHandler OnCompletion { get; set; }
        
        /// <summary>
        /// Get or set the ped for this duty.
        /// </summary>
        ACPed Ped { get; set; }

        /// <summary>
        /// Execute this duty on the <see cref="Ped"/>.
        /// This method should only be invoked by the <see cref="DutyManager"/> to prevent multi duties running at the same time on the ped.
        /// </summary>
        void Execute();

        /// <summary>
        /// End forcefully the duty if it's still active.
        /// </summary>
        void Abort();
    }
}