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
            var menuComponents = ioC.GetInstances<IMenuComponent>();

            foreach (var menuComponent in menuComponents)
            {
                menu.RegisterComponent(menuComponent);
            }

            rage.LogTrivialDebug("Registered " + menu.TotalItems + " menu component(s)");
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
    }
}