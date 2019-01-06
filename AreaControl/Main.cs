using AreaControl.AbstractionLayer;
using AreaControl.Menu;

namespace AreaControl
{
    public static class Main
    {
        /// <summary>
        /// Initialize the AreaControl plugin.
        /// This will initialize the <see cref="IMenu"/> through the <see cref="IoC"/> instance.
        /// </summary>
        public static void Initialize()
        {
            var ioC = IoC.Instance;
            var menu = ioC.GetInstance<IMenu>();
            var rage = ioC.GetInstance<IRage>();
            var menuComponents = ioC.GetInstances<IMenuComponent>();

            foreach (var menuComponent in menuComponents)
            {
                menu.RegisterComponent(menuComponent);
            }

            rage.LogTrivialDebug("Registered " + menuComponents.Count + " menu component(s)");
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