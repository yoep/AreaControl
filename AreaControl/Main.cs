using System;
using AreaControl.AbstractionLayer;
using AreaControl.AbstractionLayer.Implementation;
using AreaControl.AbstractionLayer.NoOp;
using AreaControl.Menu;
using AreaControl.Utils;

namespace AreaControl
{
    public static class Main
    {
        #region Methods

        /// <summary>
        /// Initialize the AreaControl plugin.
        /// This will initialize the <see cref="IMenu"/> through the <see cref="IoC"/> instance.
        /// </summary>
        public static void Initialize()
        {
            InitializeMenu();
            CheckDependencies();
        }

        /// <summary>
        /// Unload the plugin.
        /// </summary>
        public static void Unload()
        {
            var ioC = IoC.Instance;
            var rage = ioC.GetInstance<IRage>();
            var logger = ioC.GetInstance<ILogger>();

            try
            {
                var disposables = ioC.GetInstances<IDisposable>();

                logger.Debug("Starting disposal of " + disposables.Count + " instances");
                foreach (var instance in disposables)
                {
                    instance.Dispose();
                }

                rage.DisplayPluginNotification("~g~has been unloaded");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to unload plugin correctly with: ${ex.Message}", ex);
                rage.DisplayPluginNotification("~r~failed to unload, see logs for more info");
            }
        }

        #endregion

        #region Functions

        private static void CheckDependencies()
        {
            var ioC = IoC.Instance;
            var rage = ioC.GetInstance<IRage>();

            //wait 1 tick before asking LSPDFR for the list
            rage.NewSafeFiber(() =>
            {
                rage.FiberYield();

                //check Arrest Manager
                if (ModIntegrationUtil.IsModLoaded("Arrest Manager"))
                {
                    ioC.RegisterSingleton<IArrestManager>(typeof(ArrestManagerImpl));
                    rage.LogTrivialDebug("ArrestManagerImpl registered for Arrest Manager");
                }
                else
                {
                    rage.LogTrivial("Arrest Manager has not been loaded");
                    rage.DisplayPluginNotification("~r~Arrest Manager has not been loaded");

                    ioC.RegisterSingleton<IArrestManager>(typeof(ArrestManagerNoOp));
                    rage.LogTrivialDebug("ArrestManagerNoOp registered for Arrest Manager");
                }
            }, "Main.CheckDependencies");
        }

        private static void InitializeMenu()
        {
            var ioC = IoC.Instance;
            var menu = ioC.GetInstance<IMenu>();
            var rage = ioC.GetInstance<IRage>();
            var menuComponents = ioC.GetInstances<IMenuComponent>();

            foreach (var menuComponent in menuComponents)
            {
                menu.RegisterComponent(menuComponent);
            }

            rage.LogTrivialDebug("Registered " + menu.TotalItems + " menu component(s)");
        }

        #endregion
    }
}