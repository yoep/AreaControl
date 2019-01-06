using AreaControl;
using AreaControl.AbstractionLayer;
using AreaControlTests.Model;
using Xunit;
using Assert = Xunit.Assert;
using RageImpl = AreaControlTests.AbstractionLayer.RageImpl;

namespace AreaControlTests
{
    public static class IoCTests
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

            [Fact]
            public void ShouldAllowMultipleImplementationRegistrationOfTheSameInterface()
            {
                var ioC = IoC.Instance
                    .UnregisterAll()
                    .Register<IRage>(typeof(RageImpl))
                    .RegisterSingleton<ISingleton>(typeof(Singleton))
                    .Register<IComponent>(typeof(Component))
                    .Register<IComponent>(typeof(SameInterfaceComponent));

                var result = ioC.GetInstances<IComponent>();

                Assert.Equal(2, result.Count);
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

        public class GetInstance
        {
            [Fact]
            public void ShouldReturnPrimaryInstanceWhenDefined()
            {
                var ioC = IoC.Instance
                    .UnregisterAll()
                    .Register<IRage>(typeof(RageImpl))
                    .RegisterSingleton<ISingleton>(typeof(Singleton))
                    .Register<IComponent>(typeof(Component))
                    .Register<IComponent>(typeof(SameInterfaceComponent));

                var result = ioC.GetInstance<IComponent>();

                Assert.NotNull(result);
            }
        }

        public class GetInstances
        {
            [Fact]
            public void ShouldReturnAllInstancesThatContainTheDerivedInterfaceWhenInvoked()
            {
                var ioC = IoC.Instance
                    .UnregisterAll()
                    .Register<IRage>(typeof(RageImpl))
                    .RegisterSingleton<ISingleton>(typeof(Singleton))
                    .Register<IComponent>(typeof(Component))
                    .Register<IComponent2>(typeof(Component2));

                var result = ioC.GetInstances<IDerivedComponent>();

                Assert.Equal(2, result.Count);
                Assert.NotEqual(result[0], result[1]);
            }
        }
    }
}