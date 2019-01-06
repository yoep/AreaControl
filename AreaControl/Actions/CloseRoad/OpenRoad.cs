using AreaControl.AbstractionLayer;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.CloseRoad
{
    public class OpenRoad : IOpenRoad
    {
        private readonly IRage _rage;
        private readonly ICloseRoad _closeRoad;

        #region Constructors

        public OpenRoad(IRage rage, ICloseRoad closeRoad)
        {
            _rage = rage;
            _closeRoad = closeRoad;
        }

        #endregion

        #region IMenuComponent implementation

        /// <inheritdoc />
        public UIMenuItem Item { get; } = new UIMenuItem(AreaControl.ActionOpenRoad);

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public bool IsVisible => !_closeRoad.IsVisible;
        
        /// <inheritdoc />
        public void OnMenuActivation()
        {
            
        }

        #endregion
    }
}