using System;

namespace PlayPass.Engine
{
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