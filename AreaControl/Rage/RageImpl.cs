using System;
using Rage;

namespace AreaControl.Rage
{
    /// <inheritdoc />
    public class RageImpl : IRage
    {
        /// <inheritdoc />
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

        /// <inheritdoc />
        public void NewSafeFiber(Action action, string name)
        {
            GameFiber.StartNew(() =>
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    LogTrivial("*** An unexpected error occurred in '" + name + "' thread ***" + 
                               Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                    LogTrivial("a plugin thread has stopped working");
                }
            }, name);
        }

        /// <inheritdoc />
        public void FiberYield()
        {
            GameFiber.Yield();
        }
    }
}