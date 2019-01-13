using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using AreaControl.AbstractionLayer;
using AreaControl.Actions.CloseRoad;
using AreaControl.Debug;
using AreaControl.Duties;
using AreaControl.Instances;
using AreaControl.Menu;
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
                Main.Initialize();
                rage.LogTrivial("initialized");
                rage.DisplayNotification("has been loaded");
            }
            catch (Exception ex)
            {
                rage.LogTrivial("*** An unknown error occurred and the plugin has stopped working ***");
                rage.LogTrivial(ex.Message + Environment.NewLine + ex.StackTrace);
                rage.DisplayNotification("failed to initialize, check logs for more info");
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
                .RegisterSingleton<IMenu>(typeof(MenuImpl))
                .RegisterSingleton<IEntityManager>(typeof(EntityManager))
                .RegisterSingleton<IResponseManager>(typeof(ResponseManager))
                .RegisterSingleton<IDutyManager>(typeof(DutyManager))
                .Register<ICloseRoad>(typeof(CloseRoadImpl));
        }

        [Conditional("DEBUG")]
        private static void InitializeDebugComponents()
        {
            IoC.Instance
                .Register<ICloseRoadPreview>(typeof(CloseRoadPreview))
                .Register<IMenuComponent>(typeof(RoadInfo))
                .Register<IMenuComponent>(typeof(RoadPreview));
        }
    }
}