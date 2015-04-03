namespace PlayPass
{
    public interface ILogger
    {
        bool VerboseMode { get; set; }
        void Log(string msg);
        void LogVerbose(string msg);
    }
}