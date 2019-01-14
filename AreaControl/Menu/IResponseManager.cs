using System;
using Rage;

namespace AreaControl.Menu
{
    public interface IResponseManager : IMenuComponent, IDisposable
    {
        /// <summary>
        /// Get the current response code.
        /// </summary>
        ResponseCode ResponseCode { get; }
        
        /// <summary>
        /// Get the response code audio for the current <see cref="ResponseCode"/>.
        /// </summary>
        string ResponseCodeAudio { get; }
        
        /// <summary>
        /// Get the vehicle speed for then current response code.
        /// </summary>
        float VehicleSpeed { get; }
        
        /// <summary>
        /// Get the vehicle driving flags for the current response code.
        /// </summary>
        VehicleDrivingFlags VehicleDrivingFlags { get; }
    }
}