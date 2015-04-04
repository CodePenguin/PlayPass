using System;

namespace PlayPass.Engine
{
    /// <summary>
    ///     An interface for log classes that can handle log messages with specific depths and visibility
    /// </summary>
    public interface ILogger
    {
        bool VerboseMode { set; }
        void DecrementLogDepth(bool verboseMode);
        void IncrementLogDepth(bool verboseMode);
        void Initialize(string connectionString);
        void Log(DateTime dateTime, string msg);
        void LogException(DateTime dateTime, Exception exception);
        void LogVerbose(DateTime dateTime, string msg);
    }
}