using System;
using Rage;

namespace AreaControl.AbstractionLayer.NoOp
{
    /// <summary>
    /// No-op implementation of <see cref="IComputerPlus"/>
    /// </summary>
    public class ComputerPlusNoOp : IComputerPlus
    {
        /// <inheritdoc />
        public Guid CreateCallout(string callName, string shortName, Vector3 location, CPResponseType response)
        {
            //no-op
            return Guid.Empty;
        }

        /// <inheritdoc />
        public void UpdateCalloutStatus(Guid callId, CPCallStatus status)
        {
            //no-op
        }
    }
}