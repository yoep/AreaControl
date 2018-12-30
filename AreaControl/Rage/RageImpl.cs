using Rage;

namespace AreaControl.Rage
{
    /// <summary>
    /// Abstraction layer for calling RAGE api.
    /// This layer is created for unit tests to be able to work.
    /// </summary>
    public class RageImpl : IRage
    {
        public void DisplayNotification(string message)
        {
            Game.DisplayNotification(AreaControl.Name + " " + message.Trim());
        }

        public void LogTrivial(string message)
        {
            Game.LogTrivial(message);
        }

        public void LogTrivialDebug(string message)
        {
            Game.LogTrivialDebug(message);
        }

        public void FiberYield()
        {
            GameFiber.Yield();
        }
    }
}