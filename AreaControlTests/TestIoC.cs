using AreaControl;

namespace AreaControlTests
{
    public class TestIoC : IoC
    {
        protected TestIoC()
        {
        }

        public static TestIoC NewInstance => new TestIoC();
    }
}