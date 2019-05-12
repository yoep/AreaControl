using System.Diagnostics.CodeAnalysis;
using RAGENativeUI.Elements;

namespace AreaControl.Menu.Response
{
    public abstract class AbstractResponseSelector : IResponseSelector
    {
        private readonly IResponseManager _responseManager;

        #region Constructors

        protected AbstractResponseSelector(IResponseManager responseManager)
        {
            _responseManager = responseManager;
            MenuItem = CreateMenuItem();
        }

        #endregion

        #region IMenuComponent

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; }

        /// <inheritdoc />
        public abstract MenuType Type { get; }

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public bool IsVisible => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _responseManager.UpdateResponseCode(GetSelectedValue());
        }

        #endregion

        #region Functions

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            ((UIMenuListItem) MenuItem).OnListChanged += OnChanged;
        }

        //TODO: Synchronize menu items
        private void OnChanged(UIMenuItem sender, int newindex)
        {
            var selectedValue = GetSelectedValue();
            _responseManager.UpdateResponseCode(selectedValue);
        }

        private string GetSelectedValue()
        {
            return (string) ((UIMenuListItem) MenuItem).SelectedValue;
        }

        private UIMenuListItem CreateMenuItem()
        {
            return new UIMenuListItem("Response code", "",
                _responseManager.GetResponseCodeValue(ResponseCode.Code2),
                _responseManager.GetResponseCodeValue(ResponseCode.Code3));
        }

        #endregion
    }
}