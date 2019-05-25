using AreaControl.AbstractionLayer;
using Rage;

namespace AreaControl.Menu.Response
{
    public class ResponseManager : IResponseManager
    {
        private const string ResponseCodeValue = "Code";
        private const string RespondCodeAudio = "UNITS_RESPOND_CODE_0";
        private const float VehicleNormalSpeed = 30f;
        private const float VehicleEmergencySpeed = 45f;

        private readonly ILogger _logger;

        #region Constructors

        public ResponseManager(ILogger logger)
        {
            _logger = logger;
            ResponseCode = ResponseCode.Code2;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public ResponseCode ResponseCode { get; private set; }

        /// <inheritdoc />
        public string ResponseCodeAudio => GetResponseCodeAudio();

        /// <inheritdoc />
        public float VehicleSpeed => ResponseCode == ResponseCode.Code2 ? VehicleNormalSpeed : VehicleEmergencySpeed;

        /// <inheritdoc />
        public VehicleDrivingFlags VehicleDrivingFlags => ResponseCode == ResponseCode.Code2 ? VehicleDrivingFlags.Normal : VehicleDrivingFlags.Emergency;

        #endregion

        #region Methods

        /// <inheritdoc />
        public void UpdateResponseCode(string selectedValue)
        {
            var code = int.Parse(selectedValue.Substring(selectedValue.Length - 2));

            _logger.Trace($"Changing response code state from {ResponseCode} to {code}");
            ResponseCode = code == (int) ResponseCode.Code2 ? ResponseCode.Code2 : ResponseCode.Code3;
        }

        public string GetResponseCodeValue(ResponseCode code)
        {
            return ResponseCodeValue + " " + (int) code;
        }

        #endregion

        #region Functions

        private string GetResponseCodeAudio()
        {
            return RespondCodeAudio + (int) ResponseCode;
        }

        #endregion
    }
}