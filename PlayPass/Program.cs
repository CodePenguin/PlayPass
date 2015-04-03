using System;
using System.Diagnostics;
using System.Reflection;
using PlaySharp;

namespace PlayPass
{
    static class Program
    {
        public static void Main(string[] args)
        {
            var logManager = new LogManager();
            try
            {
                Console.WriteLine("PlayPass Auto Queueing Engine Version {0}\n", Version());

                // Initialize Queue List Factory
                FileQueueList.RegisterClass();
                MemoryQueueList.RegisterClass();

                // Initialize Queue List Factory

                var commandline = new CommandLineParser(args);
                var config = new ConfigReader(commandline.ConfigFileName);
                var queueList = QueueListFactory.GetQueueList(config.QueueListConnectionString);

                // Initialize Loggers
                logManager.Loggers.Add(new ConsoleLogger(commandline.VerboseMode));

                logManager.LogVerbose("Connecting to {0}:{1}...", config.ServerHost, config.ServerPort);
                var playOn = new PlayOn(config.ServerHost, config.ServerPort);

                var playPass = new PlayPass(playOn, logManager, queueList)
                {
                    QueueMode = commandline.QueueMode,
                    SkipMode = commandline.SkipMode
                };
                playPass.ProcessPasses(config.Passes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("The following exception has occurred:\n   " + ex.Message);
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