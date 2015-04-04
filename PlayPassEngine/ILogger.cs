using System;

namespace PlayPassEngine
{
    public interface ILogger
    {
        bool VerboseMode { get; set; }
        void Initialize(string connectionString);
        void Log(DateTime dateTime, string msg);
        void LogException(DateTime dateTime, Exception exception);
        void LogVerbose(DateTime dateTime, string msg);
    }
}