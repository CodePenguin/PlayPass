using System;
using System.Diagnostics;
using System.Reflection;
using PlayPassEngine;
using PlaySharp;

namespace PlayPass
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var logManager = new LogManager();
            try
            {
                // Initialize Queue List Factory
                FileQueueList.RegisterClass();
                MemoryQueueList.RegisterClass();

                // Initialize Logger Factory
                TextFileLogger.RegisterClass();

                var commandLine = new CommandLineParser(args);
                var config = new ConfigReader(commandLine.ConfigFileName);
                var queueList = QueueListFactory.GetQueueList(config.QueueListConnectionString);

                // Initialize Loggers
                logManager.Loggers.Add(new ConsoleLogger {VerboseMode = commandLine.VerboseMode});
                foreach (var logger in config.Loggers)
                    logManager.Loggers.Add(logger);

                logManager.Log("PlayPass Auto Queueing Engine Version {0}", Version());

                logManager.LogVerbose("Connecting to {0}:{1}...", config.ServerHost, config.ServerPort);
                var playOn = new PlayOn(config.ServerHost, config.ServerPort);

                var playPass = new PlayPassProcessor(playOn, logManager, queueList)
                {
                    QueueMode = commandLine.QueueMode,
                    SkipMode = commandLine.SkipMode
                };
                playPass.ProcessPasses(config.Passes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("The following exception has occurred:\n   " + ex.Message);
                logManager.LogException(ex);
                if (!(ex is ApplicationException))
                    Console.WriteLine("Stack Trace: " + ex);
            }
        }

        /// <summary>
        ///     Returns the version number of the program
        /// </summary>
        private static string Version()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.ProductVersion;
        }
    }
}