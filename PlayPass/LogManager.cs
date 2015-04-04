using System;
using System.Collections.Generic;
using PlayPassEngine;

namespace PlayPass
{
    internal class LogManager : ILogManager
    {
        private readonly List<ILogger> _loggers = new List<ILogger>();

        public IList<ILogger> Loggers
        {
            get { return _loggers; }
        }

        /// <summary>
        ///     Writes a log message to all the registered loggers
        /// </summary>
        public void Log(string message)
        {
            var dateTime = DateTime.Now;
            foreach (var logger in _loggers)
                logger.Log(dateTime, message);
        }

        /// <summary>
        ///     Writes a formatted log message to all the registered loggers
        /// </summary>
        public void Log(string message, params object[] args)
        {
            Log(String.Format(message, args));
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
        ///     Writes a verbose log message to all registered loggers
        /// </summary>
        public void LogVerbose(string message)
        {
            var dateTime = DateTime.Now;
            foreach (var logger in _loggers)
                logger.LogVerbose(dateTime, message);
        }

        /// <summary>
        ///     Writes a formatted verbose log message to all registered loggers
        /// </summary>
        public void LogVerbose(string message, params object[] args)
        {
            LogVerbose(String.Format(message, args));
        }
    }
}