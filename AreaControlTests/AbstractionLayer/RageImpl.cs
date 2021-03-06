using System;
using AreaControl.AbstractionLayer;

namespace AreaControlTests.AbstractionLayer
{
    public class RageImpl : IRage
    {
        public void DisplayPluginNotification(string message)
        {
            Console.WriteLine("[DisplayPluginNotification]: " + message);
        }

        public void DisplayNotification(string message)
        {
            Console.WriteLine("[DisplayNotification]: " + message);
        }

        public void LogTrivial(string message)
        {
            Console.WriteLine("[LogTrivial]: " + message);
        }

        public void LogTrivialDebug(string message)
        {
            Console.WriteLine("[LogTrivialDebug]: " + message);
        }

        public IGameFiberWrapper NewSafeFiber(Action action)
        {
            //only invoke action without being thread safe for the unit tests
            action.Invoke();

            return new TestGameFiberWrapper();
        }

        public IGameFiberWrapper NewSafeFiber(Action action, string name)
        {
            //only invoke action without being thread safe for the unit tests
            action.Invoke();

            return new TestGameFiberWrapper();
        }

        public void FiberYield()
        {
            //no-op
        }

        public void Dispose()
        {
            
        }
    }
}