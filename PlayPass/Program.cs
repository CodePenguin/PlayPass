using System;
using System.Collections.Generic;
using System.IO;

namespace PlayPass
{

    class Program
    {

        public static void Main(string[] args)
        {
            PlayPass PlayPass = new PlayPass();
            try
            {
                Console.WriteLine("PlayPass Auto Queueing Engine Version {0}\n", PlayPass.Version);
                CommandLineParser parser = new CommandLineParser(args);
                PlayPass.QueueMode = parser.QueueMode;
                PlayPass.SkipMode = parser.SkipMode;
                PlayPass.VerboseMode = parser.VerboseMode;
                PlayPass.ProcessConfigFile(parser.ConfigFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("The following exception has occurred:\n   " + ex.Message);
                if (!(ex is ApplicationException))
                    Console.WriteLine("Stack Trace: " + ex.ToString());
            }
        }

    }

}