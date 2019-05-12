using System.Diagnostics.CodeAnalysis;
using AreaControl.AbstractionLayer;
using Rage;
using RAGENativeUI.Elements;

namespace AreaControl.Menu
{
    public class ResponseManager : IResponseManager
    {
        private const string ResponseCodeValue = "Code";
        private const string RespondCodeAudio = "UNITS_RESPOND_CODE_0";
        private const float VehicleNormalSpeed = 30f;
        private const float VehicleEmergencySpeed = 45f;

        private readonly IRage _rage;
        private bool _isActive = true;

        #region Constructors

        public ResponseManager(IRage rage)
        {
            _rage = rage;
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

        #region IMenuComponent implementation

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } =
            new UIMenuListItem("Response code", "", GetResponseCodeValue(ResponseCode.Code2), GetResponseCodeValue(ResponseCode.Code3));
        
        /// <inheritdoc />
        public MenuType Type => MenuType.DEBUG;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public bool IsVisible => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            UpdateResponseCode();
        }

        /// <inheritdoc />
        public void OnMenuHighlighted(IMenu sender)
        {
            //do nothing
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            _isActive = false;
        }

        #endregion

        #region Functions

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            _rage.NewSafeFiber(() =>
            {
                while (_isActive)
                {
                    UpdateResponseCode();
                    GameFiber.Sleep(500);
                }
            }, typeof(ResponseManager).Name);
        }

        private void UpdateResponseCode()
        {
            var selectedValue = (string) ((UIMenuListItem) MenuItem).SelectedValue;
            var code = int.Parse(selectedValue.Substring(selectedValue.Length - 2));

            ResponseCode = code == (int) ResponseCode.Code2 ? ResponseCode.Code2 : ResponseCode.Code3;
        }

        private string GetResponseCodeAudio()
        {
            return RespondCodeAudio + (int) ResponseCode;
        }

        private static string GetResponseCodeValue(ResponseCode code)
        {
            return ResponseCodeValue + " " + (int) code;
        }

        #endregion
    }
}