using AreaControl.Actions.CloseRoad;
using AreaControl.Actions.RoadBlock;
using AreaControl.Menu;
using RAGENativeUI.Elements;

namespace AreaControl
{
    /// <inheritdoc />
    public class MainImpl : IMain
    {
        private readonly IMenu _menu;
        private readonly IRoadBlock _roadBlock;
        private readonly ICloseRoad _closeRoad;

        #region Constructors

        public MainImpl(IMenu menu, IRoadBlock roadBlock, ICloseRoad closeRoad)
        {
            _menu = menu;
            _roadBlock = roadBlock;
            _closeRoad = closeRoad;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Unload()
        {
            //TODO: implement unloading
        }

        #endregion

        #region Functions

        [IoC.PostConstruct]
        // ReSharper disable once UnusedMember.Local
        private void Init()
        {
            _menu.RegisterItem(new UIMenuItem("CloseRoad_Placeholder"), _closeRoad);
            _menu.RegisterItem(new UIMenuItem("RoadBlock_Placeholder"), _roadBlock);
        }

        #endregion
    }
}