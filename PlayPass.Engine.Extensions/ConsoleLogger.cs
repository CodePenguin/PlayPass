using System;

namespace PlayPass.Engine.Extensions
{
    public class ConsoleLogger : ILogger
    {
        public bool VerboseMode { get; set; }

        public void Initialize(string connectionString)
        {
            // Do nothing
        }

        public void Log(DateTime dateTime, string msg)
        {
            Console.WriteLine(msg);
        }

        public void LogException(DateTime dateTime, Exception exception)
        {
            Console.WriteLine("The following exception has occurred: " + exception.Message);
            if (!(exception is ApplicationException))
                Console.WriteLine("Stack Trace: " + exception);
        }

        public void LogVerbose(DateTime dateTime, string msg)
        {
            if (VerboseMode)
                Console.WriteLine(msg);
        }
    }
}