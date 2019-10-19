using System;
using System.Diagnostics.CodeAnalysis;
using Rage;

namespace AreaControl.AbstractionLayer
{
    /// <inheritdoc />
    /// <summary>
    /// Abstraction layer for calling RAGE api.
    /// This layer is created for unit tests to be able to work.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface IRage : IDisposable
    {
        /// <summary>
        /// Display a notification with the name of the plugin at the start.
        /// </summary>
        /// <param name="message">Set the message to show in the notification.</param>
        void DisplayPluginNotification(string message);

        /// <summary>
        /// Display a notification.
        /// </summary>
        /// <param name="message">Set the message to display in a notification.</param>
        void DisplayNotification(string message);

        void LogTrivial(string message);

        void LogTrivialDebug(string message);

        /// <summary>
        /// Start a new thread safe game fiber which will capture exceptions if they occur and log them in the console.
        /// </summary>
        /// <param name="action">Set the action to execute on the fiber.</param>
        /// <param name="name">Set the name of the new fiber (will also be used for logging).</param>
        /// <returns>Returns the game fiber wrapper which is exception safe.</returns>
        IGameFiberWrapper NewSafeFiber(Action action, string name);
        
        /// <summary>
        /// Execute GameFiber.Yield in rage
        /// </summary>
        void FiberYield();
    }
}