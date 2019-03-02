using System;
using System.Reflection;
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
            CheckDependencies();
            InitializeMenu();
        }

        /// <summary>
        /// Unload the plugin.
        /// </summary>
        public static void Unload()
        {
            var ioC = IoC.Instance;
            var rage = ioC.GetInstance<IRage>();

            try
            {
                var disposables = ioC.GetInstances<IDisposable>();

                rage.LogTrivialDebug("Starting disposal of " + disposables.Count + " instances");
                foreach (var instance in disposables)
                {
                    instance.Dispose();
                }
            }
            catch (Exception ex)
            {
                rage.LogTrivial("Failed to unload plugin correctly with: " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        #endregion

        #region Functions

        private static void CheckDependencies()
        {
            var ioC = IoC.Instance;
            var rage = ioC.GetInstance<IRage>();

            //check Arrest Manager
            if (ModIntegrationUtil.IsModLoaded(Assembly.GetAssembly(typeof(Arrest_Manager.API.Functions))))
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