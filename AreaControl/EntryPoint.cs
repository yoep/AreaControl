using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using AreaControl.AbstractionLayer;
using AreaControl.AbstractionLayer.Implementation;
using AreaControl.Actions.CleanArea;
using AreaControl.Actions.CloseRoad;
using AreaControl.Actions.RedirectTraffic;
using AreaControl.Actions.SlowDownTraffic;
using AreaControl.Debug;
using AreaControl.Duties;
using AreaControl.Instances;
using AreaControl.Menu;
using AreaControl.Settings;
using LSPD_First_Response.Mod.API;

namespace AreaControl
{
    /// <inheritdoc />
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class EntryPoint : Plugin
    {
        /// <inheritdoc />
        public override void Initialize()
        {
            InitializeIoContainer();
            InitializeDebugComponents();
            var rage = IoC.Instance.GetInstance<IRage>();

            try
            {
                AttachDebugger();
                
                Main.Initialize();
                rage.LogTrivial("initialized");
                rage.DisplayPluginNotification("has been loaded");
            }
            catch (Exception ex)
            {
                rage.LogTrivial("*** An unknown error occurred and the plugin has stopped working ***");
                rage.LogTrivial(ex.Message + Environment.NewLine + ex.StackTrace);
                rage.DisplayPluginNotification("~r~failed to initialize, check logs for more info");
            }
        }

        /// <inheritdoc />
        public override void Finally()
        {
            Main.Unload();
        }

        private static void InitializeIoContainer()
        {
            IoC.Instance
                .Register<IRage>(typeof(RageImpl))
                .Register<ILogger>(typeof(Logger))
                .RegisterSingleton<IMenu>(typeof(MenuImpl))
                .RegisterSingleton<IEntityManager>(typeof(EntityManager))
                .RegisterSingleton<IResponseManager>(typeof(ResponseManager))
                .RegisterSingleton<IDutyManager>(typeof(DutyManager))
                .RegisterSingleton<ISettingsManager>(typeof(SettingsManager))
                .Register<ICleanArea>(typeof(CleanAreaImpl))
                .Register<ICloseRoad>(typeof(CloseRoadImpl))
                .Register<IRedirectTraffic>(typeof(RedirectTrafficImpl))
                .Register<ISlowDownTraffic>(typeof(SlowDownTrafficImpl));
        }

        [Conditional("DEBUG")]
        private static void InitializeDebugComponents()
        {
            IoC.Instance
                .Register<ICloseRoadPreview>(typeof(CloseRoadPreview))
                .Register<IMenuComponent>(typeof(RoadInfo))
                .Register<IMenuComponent>(typeof(RoadPreview))
                .Register<IMenuComponent>(typeof(RedirectTrafficPreview));
        }

        [Conditional("DEBUG")]
        private static void AttachDebugger()
        {
            Rage.Debug.AttachAndBreak();
        }
    }
}