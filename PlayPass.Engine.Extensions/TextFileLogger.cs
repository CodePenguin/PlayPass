using System;
using System.Data.Common;
using System.IO;

namespace PlayPass.Engine.Extensions
{
    public class TextFileLogger : ILogger
    {
        private StreamWriter _file;
        private int _logDepth;
        public bool VerboseMode { private get; set; }

        public void DecrementLogDepth(bool verboseMode)
        {
            if (!verboseMode || VerboseMode)
                _logDepth--;
        }

        public void IncrementLogDepth(bool verboseMode)
        {
            if (!verboseMode || VerboseMode)
                _logDepth++;
        }

        public void Initialize(string connectionString)
        {
            var parser = new DbConnectionStringBuilder {ConnectionString = connectionString};
            var fileName = parser.ContainsKey("Filename")
                ? parser["Filename"].ToString()
                : Path.Combine(Directory.GetCurrentDirectory(), "PlayPass.log");
            var appendMode = !parser.ContainsKey("Append") || (parser["Append"].ToString() == "1");
            _file = new StreamWriter(fileName, appendMode) {AutoFlush = true};
        }

        public void Log(DateTime dateTime, string msg)
        {
            _file.WriteLine("{0}:{1}{2}", dateTime.ToString("u"), Padding(), msg);
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

        private string Padding()
        {
            return new String('\t', _logDepth + 1);
        }

        /// <summary>
        ///     Registers this class with the LoggerFactory
        /// </summary>
        public static void RegisterClass()
        {
            LoggerFactory.RegisterClass(typeof (TextFileLogger));
        }
    }
}