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
            Game.DisplayNotification("~b~" + AreaControl.Name + " ~s~" + message.Trim());
        }

        public void LogTrivial(string message)
        {
            Game.LogTrivial("[" + AreaControl.Name + "]: " + message.Trim());
        }

        public void LogTrivialDebug(string message)
        {
            Game.LogTrivialDebug("[" + AreaControl.Name + "]: " + message.Trim());
        }

        public void FiberYield()
        {
            GameFiber.Yield();
        }
    }
}