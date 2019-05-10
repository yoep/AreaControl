using System;

namespace AreaControl.AbstractionLayer.Implementation
{
    public class Logger : ILogger
    {
        private const string LevelWarn = "WARN";
        private const string LevelError = "ERROR";
        
        private readonly IRage _rage;

        public Logger(IRage rage)
        {
            _rage = rage;
        }

        public void Trace(string message)
        {
            _rage.LogTrivialDebug(BuildMessage("TRACE", message));
        }

        public void Debug(string message)
        {
            _rage.LogTrivialDebug(BuildMessage("DEBUG", message));
        }

        public void Info(string message)
        {
            _rage.LogTrivial(BuildMessage("INFO", message));
        }

        public void Warn(string message)
        {
            _rage.LogTrivial(BuildMessage(LevelWarn, message));
        }

        public void Warn(string message, Exception exception)
        {
            _rage.LogTrivial(BuildMessage(LevelWarn, message, exception));
        }

        public void Error(string message)
        {
            _rage.LogTrivial(BuildMessage(LevelError, message));
        }

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