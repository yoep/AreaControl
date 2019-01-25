using System;
using System.Threading;
using Rage;

namespace AreaControl.AbstractionLayer
{
    /// <inheritdoc />
    public class RageImpl : IRage
    {
        /// <inheritdoc />
        public void DisplayPluginNotification(string message)
        {
            Game.DisplayNotification("~b~" + AreaControl.Name + " ~s~" + message.Trim());
        }

        /// <inheritdoc />
        public void DisplayNotification(string message)
        {
            Game.DisplayNotification(message.Trim());
        }

        /// <inheritdoc />
        public void LogTrivial(string message)
        {
            Game.LogTrivial("[" + AreaControl.Name + "]: " + message.Trim());
        }

        /// <inheritdoc />
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
                catch (ThreadInterruptedException)
                {
                    //ignore as this is probably on plugin termination and thread is in waiting state
                }
                catch (ThreadAbortException)
                {
                    //ignore as this is probably on plugin termination and thread was executing a method and couldn't exit correctly
                }
                catch (Exception ex)
                {
                    LogTrivial("*** An unexpected error occurred in '" + name + "' thread ***" +
                               Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                    DisplayPluginNotification("~r~" + name + " thread has stopped working, see logs for more info");
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