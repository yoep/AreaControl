using AreaControl.Menu;
using LSPD_First_Response.Mod.API;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.CloseRoad
{
    public class OpenRoad : IMenuComponent
    {
        private readonly ICloseRoad _closeRoad;

        #region Constructors

        public OpenRoad(ICloseRoad closeRoad)
        {
            _closeRoad = closeRoad;
        }

        #endregion

        #region IMenuComponent implementation

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.ActionOpenRoad);

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public bool IsVisible => _closeRoad.IsActive;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            Functions.PlayScannerAudio("WE_ARE_CODE_4");
            _closeRoad.OpenRoad();
            //register a new close road
            sender.ReplaceComponent(this, IoC.Instance.GetInstance<ICloseRoad>());
        }

        #endregion
    }
}