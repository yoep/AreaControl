using Rage;

namespace AreaControl.Menu.Response
{
    public interface IResponseManager
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

        string GetResponseCodeValue(ResponseCode code);

        /// <summary>
        /// Update the response code.
        /// </summary>
        /// <param name="selectedValue">The selected value in the menu.</param>
        void UpdateResponseCode(string selectedValue);
    }
}