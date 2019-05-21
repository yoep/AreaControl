using System;

namespace AreaControl.AbstractionLayer
{
    public interface ILogger
    {
        /// <summary>
        /// Log a message with the level TRACE.
        /// This message will only be logged when DEBUG mode is activated in Rage.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void Trace(string message);
        
        /// <summary>
        /// Log a message with the level DEBUG.
        /// This message will only be logged when DEBUG mode is activated in Rage.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void Debug(string message);
        
        /// <summary>
        /// Log a message with the level INFO.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void Info(string message);

        /// <summary>
        /// Log a message with the level WARN.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void Warn(string message);

        /// <summary>
        /// Log a message and exception with the level WARN.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">The exception to log.</param>
        void Warn(string message, Exception exception);
        
        /// <summary>
        /// Log a message with the level ERROR.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void Error(string message);
        
        /// <summary>
        /// Log a message and exception with the level ERROR.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">The exception to log.</param>
        void Error(string message, Exception exception);
    }
}