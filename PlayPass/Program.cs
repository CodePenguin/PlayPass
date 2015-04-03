using System;
using System.Diagnostics;
using System.Reflection;

namespace PlayPass
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var logManager = new LogManager();
            var playPass = new PlayPass(logManager);
            try
            {
                Console.WriteLine("PlayPass Auto Queueing Engine Version {0}\n", Version());
                var parser = new CommandLineParser(args);

                logManager.Loggers.Add(new ConsoleLogger(parser.VerboseMode));

                playPass.QueueMode = parser.QueueMode;
                playPass.SkipMode = parser.SkipMode;
                playPass.VerboseMode = parser.VerboseMode;
                playPass.ProcessConfigFile(parser.ConfigFileName);
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
        public static string Version()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.ProductVersion;
        }

    }
}