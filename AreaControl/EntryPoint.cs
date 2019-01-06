using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using AreaControl.AbstractionLayer;
using AreaControl.Actions.CloseRoad;
using AreaControl.Actions.RoadBlock;
using AreaControl.Managers;
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
                rage.DisplayNotification("an unknown error occurred and the plugin has stopped working");
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
                .Register<IRoadBlock>(typeof(RoadBlockImpl))
                .Register<ICloseRoad>(typeof(CloseRoadImpl));
        }

        [Conditional("DEBUG")]
        private static void InitializeDebugComponents()
        {
            IoC.Instance
                .Register<ICloseRoad>(typeof(CloseRoadPreview));
        }
    }
}