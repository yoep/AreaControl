using AreaControl.Menu;
using AreaControl.Rage;
using AreaControl.Utils;
using Xunit;
using RageImpl = AreaControlTests.Rage.RageImpl;

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
                    .RegisterSingleton<IMenu>(typeof(MenuImpl));

                var result = ioC.GetInstance<IMenu>();
                
                Assert.NotNull(result);
                Assert.True(result.IsMenuInitialized);
            }
        }
    }
}