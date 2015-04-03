using System;
using System.Collections.Generic;

namespace PlayPass
{
    internal class LogManager
    {
        private readonly List<ILogger> _loggers = new List<ILogger>();

        public List<ILogger> Loggers
        {
            get { return _loggers; }
        }

        /// <summary>
        ///     Writes a log message to all the registered loggers
        /// </summary>
        public void Log(string message)
        {
            foreach (var logger in _loggers)
                logger.Log(message);
        }

        /// <summary>
        ///     Writes a formatted log message to all the registered loggers
        /// </summary>
        public void Log(string message, params object[] args)
        {
            Log(String.Format(message, args));
        }

        /// <summary>
        ///     Writes a verbose log message to all registered loggers
        /// </summary>
        public void LogVerbose(string message)
        {
            foreach (var logger in _loggers)
                logger.LogVerbose(message);
        }

        /// <summary>
        ///     Writes a formatted verbose log message to all registered loggers
        /// </summary>
        public void LogVerbose(string message, params object[] args)
        {
            message = String.Format(message, args);
            foreach (var logger in _loggers)
                logger.LogVerbose(message);
        }
    }
}