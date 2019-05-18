using System;
using ComputerPlus;
using ComputerPlus.API;
using Rage;

namespace AreaControl.AbstractionLayer.Implementation
{
    /// <summary>
    /// Wrapper implementation of <see cref="IComputerPlus"/>
    /// </summary>
    public class ComputerPlusImpl : IComputerPlus
    {
        /// <inheritdoc />
        public Guid CreateCallout(string callName, string shortName, Vector3 location, CPResponseType response)
        {
            return Functions.CreateCallout(new CalloutData(callName, shortName, location, ConvertResponseType(response)));
        }

        /// <inheritdoc />
        public void UpdateCalloutStatus(Guid callId, CPCallStatus status)
        {
            Functions.UpdateCalloutStatus(callId, ConvertCallStatus(status));
        }

        private static ECallStatus ConvertCallStatus(CPCallStatus status)
        {
            return (ECallStatus) (int) status;
        }

        private static EResponseType ConvertResponseType(CPResponseType response)
        {
            return (EResponseType) (int) response;
        }
    }
}