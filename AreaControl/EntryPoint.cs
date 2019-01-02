using System;
using System.Diagnostics.CodeAnalysis;
using AreaControl.Actions.CloseRoad;
using AreaControl.Actions.RoadBlock;
using AreaControl.Menu;
using AreaControl.Rage;
using AreaControl.Utils;
using AreaControl.Utils.Query;
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

            try
            {
                var ioC = IoC.Instance;
                var rage = ioC.GetInstance<IRage>();

                //IMain will automatically initialize all different components of this plugin through the IoC
                ioC.GetInstance<IMain>();

                rage.LogTrivial("initialized");
                rage.DisplayNotification("has been loaded");
            }
            catch (Exception ex)
            {
                var rage = IoC.Instance.GetInstance<IRage>();
                rage.LogTrivial("*** An unknown error occurred and the plugin has stopped working ***");
                rage.LogTrivial(ex.Message + Environment.NewLine + ex.StackTrace);
                rage.DisplayNotification("an unknown error occurred and the plugin has stopped working");
            }
        }

        /// <inheritdoc />
        public override void Finally()
        {
            IoC.Instance.GetInstance<IMain>().Unload();
        }

        private static void InitializeIoContainer()
        {
            IoC.Instance
                .Register<IRage>(typeof(RageImpl))
                .RegisterSingleton<IMain>(typeof(MainImpl))
                .RegisterSingleton<IMenu>(typeof(MenuImpl))
                .RegisterSingleton<IRoadUtil>(typeof(RoadUtil))
                .Register<IRoadBlock>(typeof(RoadBlockImpl))
                .Register<ICloseRoad>(typeof(CloseRoadImpl))
                .Register<IPedQuery>(typeof(PedQuery))
                .Register<IVehicleQuery>(typeof(VehicleQuery));
        }
    }
}