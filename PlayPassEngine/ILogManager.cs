using System.Collections.Generic;

namespace PlayPassEngine
{
    public interface ILogManager
    {
        IList<ILogger> Loggers { get; }

        /// <summary>
        ///     Writes a log message to all the registered loggers
        /// </summary>
        void Log(string message);

        /// <summary>
        ///     Writes a formatted log message to all the registered loggers
        /// </summary>
        void Log(string message, params object[] args);

        /// <summary>
        ///     Writes a verbose log message to all registered loggers
        /// </summary>
        void LogVerbose(string message);

        /// <summary>
        ///     Writes a formatted verbose log message to all registered loggers
        /// </summary>
        void LogVerbose(string message, params object[] args);

    }
}
