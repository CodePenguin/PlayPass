using System;
using System.Collections.Generic;

namespace PlayPassEngine
{
    public interface ILogManager
    {
        IList<ILogger> Loggers { get; }

        /// <summary>
        ///     Logs a message to all the registered loggers
        /// </summary>
        void Log(string message);

        /// <summary>
        ///     Logs a formatted message to all the registered loggers
        /// </summary>
        void Log(string message, params object[] args);

        /// <summary>
        ///     Logs an exception to all registered loggers
        /// </summary>
        void LogException(Exception exception);

        /// <summary>
        ///     Logs a verbose message to all registered loggers
        /// </summary>
        void LogVerbose(string message);

        /// <summary>
        ///     Logs a formatted verbose message to all registered loggers
        /// </summary>
        void LogVerbose(string message, params object[] args);

    }
}
