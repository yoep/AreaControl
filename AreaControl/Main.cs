using System;
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

            try
            {
                var menuComponents = ioC.GetInstances<IMenuComponent>();
                
                foreach (var menuComponent in menuComponents)
                {
                    menu.RegisterComponent(menuComponent);
                }
                
                rage.LogTrivialDebug("Registered " + menuComponents.Count + " menu component(s)");
            }
            catch (Exception ex)
            {
                rage.LogTrivial("*** An unexpected error occurred during initialization ***" +
                                Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                rage.LogTrivial("failed to initialize");
            }
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