using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using AreaControl.AbstractionLayer;
using AreaControl.AbstractionLayer.Implementation;
using AreaControl.Actions.CleanArea;
using AreaControl.Actions.CloseRoad;
using AreaControl.Actions.CrimeScene;
using AreaControl.Actions.RedirectTraffic;
using AreaControl.Actions.TrafficBreak;
using AreaControl.Callouts;
using AreaControl.Debug;
using AreaControl.Duties;
using AreaControl.Instances;
using AreaControl.Menu;
using AreaControl.Menu.Response;
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
            var logger = IoC.Instance.GetInstance<ILogger>();

            try
            {
                AttachDebugger();

                Main.Initialize();
                logger.Info("Initialized");
                rage.DisplayPluginNotification("has been loaded");

                Functions.OnOnDutyStateChanged += OnDutyStateChanged;
            }
            catch (Exception ex)
            {
                logger.Error($"An unexpected error occurred causing the plugin to stop with error: {ex.Message}", ex);
                rage.DisplayPluginNotification("~r~failed to initialize, check logs for more info");
            }
        }

        /// <inheritdoc />
        public override void Finally()
        {
            Main.Unload();
        }

        private static void OnDutyStateChanged(bool isOnDuty)
        {
            if (!isOnDuty)
                Main.Unload();
        }

        private static void InitializeIoContainer()
        {
            IoC.Instance
                .UnregisterAll()
                .Register<IRage>(typeof(RageImpl))
                .Register<ILogger>(typeof(Logger))
                .RegisterSingleton<IMenu>(typeof(MenuImpl))
                .RegisterSingleton<IEntityManager>(typeof(EntityManager))
                .RegisterSingleton<IResponseManager>(typeof(ResponseManager))
                .RegisterSingleton<IDutyManager>(typeof(DutyManager))
                .RegisterSingleton<ISettingsManager>(typeof(SettingsManager))
                .RegisterSingleton<ICalloutManager>(typeof(CalloutManager))
                .Register<IResponseSelector>(typeof(StreetControlResponseSelector))
                .Register<ICleanArea>(typeof(CleanAreaImpl))
                .Register<ICrimeScene>(typeof(CrimeSceneImpl))
                .Register<ICloseRoad>(typeof(CloseRoadImpl))
                .Register<IRedirectTraffic>(typeof(RedirectTrafficImpl))
                .Register<ITrafficBreak>(typeof(TrafficBreakImpl));
        }

        [Conditional("DEBUG")]
        private static void InitializeDebugComponents()
        {
            IoC.Instance
                .RegisterSingleton<ICloseRoadPreview>(typeof(CloseRoadPreview))
                .RegisterSingleton<IRedirectTrafficPreview>(typeof(RedirectTrafficPreview))
                .RegisterSingleton<ICrimeScenePreview>(typeof(CrimeScenePreview))
                .RegisterSingleton<IRoadInfo>(typeof(RoadInfo))
                .Register<IRoadPreview>(typeof(RoadPreview));
        }

        [Conditional("DEBUG")]
        private static void AttachDebugger()
        {
            Rage.Debug.AttachAndBreak();
        }
    }
}