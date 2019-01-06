using AreaControl;
using AreaControl.AbstractionLayer;
using AreaControlTests.Model;
using Xunit;
using Assert = Xunit.Assert;
using RageImpl = AreaControlTests.AbstractionLayer.RageImpl;

namespace AreaControlTests
{
    public class IoCTests
    {
        public class Register
        {
            [Fact]
            public void ShouldReturnDifferentInstances()
            {
                var ioC = IoC.Instance
                    .UnregisterAll()
                    .Register<IRage>(typeof(RageImpl))
                    .RegisterSingleton<ISingleton>(typeof(Singleton))
                    .Register<IComponent>(typeof(Component));
                var expectedResult = ioC.GetInstance<IComponent>();

                var result = ioC.GetInstance<IComponent>();

                Assert.NotEqual(expectedResult, result);
            }
        }

        public class RegisterSingleton
        {
            [Fact]
            public void ShouldReturnSingletonInstance()
            {
                var ioC = IoC.Instance
                    .UnregisterAll()
                    .Register<IRage>(typeof(RageImpl))
                    .RegisterSingleton<ISingleton>(typeof(Singleton))
                    .Register<IComponent>(typeof(Component));
                var expectedResult = ioC.GetInstance<ISingleton>();

                var result = ioC.GetInstance<ISingleton>();

                Assert.Equal(expectedResult, result);
            }
        }

        public class PostConstruct
        {
            [Fact]
            public void ShouldInvokePostConstructMethod()
            {
                var ioC = IoC.Instance;
                ioC
                    .UnregisterAll()
                    .RegisterSingleton<IPostConstructModel>(typeof(PostConstructModel));

                var result = ioC.GetInstance<IPostConstructModel>();

                Assert.NotNull(result);
                Assert.True(result.IsInitialized);
            }
        }

        public class UnregisterAll
        {
            [Fact]
            public void ShouldCleanIoCWhenInvoked()
            {
                var ioC = IoC.Instance
                    .UnregisterAll()
                    .Register<IRage>(typeof(RageImpl))
                    .RegisterSingleton<ISingleton>(typeof(Singleton))
                    .Register<IComponent>(typeof(Component));
                ioC.GetInstance<ISingleton>();

                ioC.UnregisterAll();

                Assert.Throws<IoCException>(() => ioC.InstanceExists<ISingleton>());
            }
        }
    }
}