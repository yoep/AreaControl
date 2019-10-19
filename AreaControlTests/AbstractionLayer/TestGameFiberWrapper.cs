using AreaControl.AbstractionLayer;

namespace AreaControlTests.AbstractionLayer
{
    public class TestGameFiberWrapper : IGameFiberWrapper
    {
        public string Name { get; }
        public bool IsAlive { get; }

        public void Abort()
        {
        }
    }
}