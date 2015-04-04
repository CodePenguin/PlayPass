using System;
using System.Data.Common;
using System.IO;
using System.Security.Policy;
using PlayPassEngine;
using PlaySharp;

namespace PlayPass
{
    class TextFileLogger : ILogger
    {
        private StreamWriter _file;
        public bool VerboseMode { get; set; }

        /// <summary>
        ///     Registers this class with the LoggerFactory
        /// </summary>
        public static void RegisterClass()
        {
            LoggerFactory.RegisterClass(typeof(TextFileLogger));
        }

        public void Initialize(string connectionString)
        {
            var parser = new DbConnectionStringBuilder() { ConnectionString = connectionString };
            var fileName = parser.ContainsKey("Filename") ? parser["Filename"].ToString() : Path.Combine(Directory.GetCurrentDirectory(), "PlayPass.log");
            var appendMode = !parser.ContainsKey("Append") || (parser["Append"].ToString() == "1");
            _file = new StreamWriter(fileName, appendMode) { AutoFlush = true };
        }

        public void Log(DateTime dateTime, string msg)
        {
            _file.WriteLine("{0} {1}: {2}", dateTime.ToShortDateString(), dateTime.ToLongTimeString(), msg);
        }

        public void LogException(DateTime dateTime, Exception exception)
        {
            var msg = "The following exception has occurred: " + exception.Message;
            if (!(exception is ApplicationException))
                msg += "\nStack Trace: " + exception;
            Log(dateTime, msg);
        }

        public void LogVerbose(DateTime dateTime, string msg)
        {
            if (VerboseMode)
                Log(dateTime, msg);
        }

    }
}
