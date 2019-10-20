using AreaControl;

namespace AreaControlTests.TestExtension
{
    public class TestIoC : IoC
    {
        protected TestIoC()
        {
        }

        public static TestIoC NewInstance => new TestIoC();
    }
}