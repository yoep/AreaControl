using AreaControl.Actions.CloseRoad;
using AreaControl.Actions.RoadBlock;
using AreaControl.Menu;
using RAGENativeUI.Elements;

namespace AreaControl
{
    public static class Main
    {
        /// <summary>
        /// Initialize the plugin.
        /// </summary>
        public static void Initialize()
        {
            var ioC = IoC.Instance;
            var menu = ioC.GetInstance<IMenu>();

            menu.RegisterItem(new UIMenuItem("CloseRoad_Placeholder"), ioC.GetInstance<ICloseRoad>());
            menu.RegisterItem(new UIMenuItem("RoadBlock_Placeholder"), ioC.GetInstance<IRoadBlock>());
        }

        /// <summary>
        /// Unload the plugin.
        /// </summary>
        public static void Unload()
        {
            //TODO: implement unloading of components
        }
    }
}