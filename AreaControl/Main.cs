using System;
using AreaControl.AbstractionLayer;
using AreaControl.AbstractionLayer.Implementation;
using AreaControl.AbstractionLayer.NoOp;
using AreaControl.Callouts;
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
            InitializeCallouts();
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
            var logger = ioC.GetInstance<ILogger>();

            // wait 1 tick before asking LSPDFR for the list
            rage.NewSafeFiber(() =>
            {
                rage.FiberYield();

                // check Arrest Manager
                if (ModIntegrationUtil.IsModLoaded("Arrest Manager"))
                {
                    ioC.RegisterSingleton<IArrestManager>(typeof(ArrestManagerImpl));
                    logger.Info("ArrestManagerImpl registered for Arrest Manager");
                }
                else
                {
                    logger.Warn("Arrest Manager has not been loaded");
                    rage.DisplayPluginNotification("~r~Arrest Manager has not been loaded");

                    ioC.RegisterSingleton<IArrestManager>(typeof(ArrestManagerNoOp));
                    logger.Info("ArrestManagerNoOp registered for Arrest Manager");
                }

                // check Computer+
                if (ModIntegrationUtil.IsModLoaded("ComputerPlus"))
                {
                    ioC.RegisterSingleton<IComputerPlus>(typeof(ComputerPlusImpl));
                    logger.Info("ComputerPlusImpl registered for Arrest Manager");
                }
                else
                {
                    logger.Warn("Computer+ has not been loaded");
                    rage.DisplayPluginNotification("~r~Computer+ has not been loaded");

                    ioC.RegisterSingleton<IComputerPlus>(typeof(ComputerPlusNoOp));
                    logger.Info("ComputerPlusNoOp registered for Arrest Manager");
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

        private static void InitializeCallouts()
        {
            IoC.Instance.GetInstance<ICalloutManager>();
        }

        #endregion
    }
}