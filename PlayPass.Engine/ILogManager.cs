using System;

namespace PlayPass.Engine
{
    /// <summary>
    ///     An interface for logging to multiple loggers that can handle log messages with specific depths and visibility
    /// </summary>
    public interface ILogManager
    {
        /// <summary>
        ///     Returns a an object to automatically increase and decrease the log depth
        /// </summary>
        IDisposable NextLogDepth();

        /// <summary>
        ///     Returns a an object to automatically increase and decrease the log depth when in verbose mode
        /// </summary>
        IDisposable NextLogVerboseDepth();

        /// <summary>
        ///     Logs a formatted message to all the registered loggers
        /// </summary>
        void Log(string message, params object[] args);

        /// <summary>
        ///     Logs an exception to all registered loggers
        /// </summary>
        void LogException(Exception exception);

        /// <summary>
        ///     Logs a formatted verbose message to all registered loggers
        /// </summary>
        void LogVerbose(string message, params object[] args);
    }   

}