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

        public ResponseCode ResponseCode { get; private set; }

        public string ResponseCodeAudio => GetResponseCodeAudio();

        #endregion

        #region IMenuComponent implementation

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } =
            new UIMenuListItem("Response code", "", GetResponseCodeValue(ResponseCode.Code2), GetResponseCodeValue(ResponseCode.Code3));

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public bool IsVisible => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            UpdateResponseCode();
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