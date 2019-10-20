using System;
using AreaControl.AbstractionLayer;

namespace AreaControlTests.AbstractionLayer
{
    public class LoggerImpl : ILogger
    {
        private const string LevelTrace = "TRACE";
        private const string LevelDebug = "DEBUG";
        private const string LevelInfo = "INFO";
        private const string LevelWarn = "WARN";
        private const string LevelError = "ERROR";

        private readonly IRage _rage;

        public LoggerImpl(IRage rage)
        {
            _rage = rage;
        }

        /// <inheritdoc />
        public void Trace(string message)
        {
            _rage.LogTrivialDebug(BuildMessage(LevelTrace, message));
        }

        /// <inheritdoc />
        public void Debug(string message)
        {
            _rage.LogTrivialDebug(BuildMessage(LevelDebug, message));
        }

        /// <inheritdoc />
        public void Info(string message)
        {
            _rage.LogTrivial(BuildMessage(LevelInfo, message));
        }

        /// <inheritdoc />
        public void Warn(string message)
        {
            _rage.LogTrivial(BuildMessage(LevelWarn, message));
        }

        /// <inheritdoc />
        public void Warn(string message, Exception exception)
        {
            _rage.LogTrivial(BuildMessage(LevelWarn, message, exception));
        }

        /// <inheritdoc />
        public void Error(string message)
        {
            _rage.LogTrivial(BuildMessage(LevelError, message));
        }

        /// <inheritdoc />
        public void Error(string message, Exception exception)
        {
            _rage.LogTrivial(BuildMessage(LevelError, message, exception));
        }

        private static string BuildMessage(string level, string message, Exception exception = null)
        {
            var stacktrace = exception?.StackTrace;

            return $"[{level}] {message}\n{stacktrace}";
        }
    }
}