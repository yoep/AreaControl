using AreaControl;
using AreaControl.AbstractionLayer;
using AreaControl.Menu;
using AreaControl.Settings;
using AreaControlTests.AbstractionLayer;
using Xunit;
using Assert = Xunit.Assert;

namespace AreaControlTests
{
    public static class MenuImplTests
    {
        public class Init
        {
            [Fact]
            public void ShouldInitializeMenuPoolWhenInvoked()
            {
                var ioC = IoC.Instance;
                ioC.UnregisterAll()
                    .Register<IRage>(typeof(RageImpl))
                    .Register<ISettingsManager>(typeof(SettingsManagerImpl))
                    .RegisterSingleton<IMenu>(typeof(MenuImpl));

                var result = ioC.GetInstance<IMenu>();

                Assert.NotNull(result);
                Assert.True(result.IsMenuInitialized);
            }
        }
    }
}