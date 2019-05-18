using System;
using Rage;

namespace AreaControl.AbstractionLayer
{
    public interface IComputerPlus
    {
        /// <summary>
        /// Create a new Computer+ callout.
        /// </summary>
        /// <param name="callName">The name of the callout.</param>
        /// <param name="shortName">The short name of the callout.</param>
        /// <param name="location">The location of the callout.</param>
        /// <param name="response">The response type of the callout.</param>
        /// <returns>Returns the GUID of the created callout.</returns>
        Guid CreateCallout(string callName, string shortName, Vector3 location, CPResponseType response);

        /// <summary>
        /// Update the given callout with the new status.
        /// </summary>
        /// <param name="callId">The callout ID to update.</param>
        /// <param name="status">The new status of the callout.</param>
        void UpdateCalloutStatus(Guid callId, CPCallStatus status);
    }
}