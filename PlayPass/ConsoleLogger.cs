using System;

namespace PlayPass
{
    internal class ConsoleLogger : ILogger
    {
        public bool VerboseMode { get; set; }

        public ConsoleLogger(bool verboseMode)
        {
            VerboseMode = verboseMode;
        }

        public void Log(string msg)
        {
            Console.WriteLine(msg);
        }

        public void LogVerbose(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}