using System;
using System.Collections.Generic;

namespace PlayPass.Engine.Extensions
{
    public class LogManager : ILogManager
    {
        private readonly List<ILogger> _loggers = new List<ILogger>();

        public IList<ILogger> Loggers
        {
            get { return _loggers; }
        }

        /// <summary>
        ///     Decrements the current log depth
        /// </summary>
        private void DecrementLogDepth(bool verboseMode)
        {
            foreach (var logger in _loggers)
                logger.DecrementLogDepth(verboseMode);
        }

        /// <summary>
        ///     Increments the current log depth
        /// </summary>
        private void IncrementLogDepth(bool verboseMode)
        {
            foreach (var logger in _loggers)
                logger.IncrementLogDepth(verboseMode);
        }

        /// <summary>
        ///     Writes a formatted log message to all the registered loggers
        /// </summary>
        public void Log(string message, params object[] args)
        {
            var dateTime = DateTime.Now;
            var msg = String.Format(message, args);
            foreach (var logger in _loggers)
                logger.Log(dateTime, msg);
        }

        /// <summary>
        ///     Logs an exception to all registered loggers
        /// </summary>
        public void LogException(Exception exception)
        {
            var dateTime = DateTime.Now;
            foreach (var logger in _loggers)
                logger.LogException(dateTime, exception);
        }

        /// <summary>
        ///     Writes a formatted verbose log message to all registered loggers
        /// </summary>
        public void LogVerbose(string message, params object[] args)
        {
            var dateTime = DateTime.Now;
            var msg = String.Format(message, args);
            foreach (var logger in _loggers)
                logger.LogVerbose(dateTime, msg);
        }

        public IDisposable NextLogDepth()
        {
            return new LogDepthPointer(this, false);
        }

        public IDisposable NextLogVerboseDepth()
        {
            return new LogDepthPointer(this, true);
        }

        private class LogDepthPointer : IDisposable
        {
            private readonly LogManager _logManager;
            private readonly bool _verboseMode;

            public LogDepthPointer(LogManager logManager, bool verboseMode)
            {
                _verboseMode = verboseMode;
                _logManager = logManager;
                _logManager.IncrementLogDepth(_verboseMode);
            }

            public void Dispose()
            {
                _logManager.DecrementLogDepth(_verboseMode);
            }
        }
    }
}