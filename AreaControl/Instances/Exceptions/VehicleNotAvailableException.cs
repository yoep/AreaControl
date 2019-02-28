using System;

namespace AreaControl.Instances.Exceptions
{
    /// <inheritdoc />
    /// <summary>
    /// Defines that the vehicle that is trying to be accessed is unavailable.
    /// </summary>
    public class VehicleNotAvailableException : Exception
    {
        public VehicleNotAvailableException(string message)
            : base(message)
        {
        }
    }
}