using System;
using System.IO;

namespace PlayPass
{

    class Program
    {
        public static void Main(string[] args)
        {
            string ConfigFileName;
            bool QueueMode = false;

            Console.WriteLine("PlayPass Auto Queueing Engine\n");

            if (args.Length == 1)
                ConfigFileName = Path.Combine(Directory.GetCurrentDirectory(), "PlayPass.cfg");
            else if (args.Length > 2)
                throw new Exception("Invalid command line arguments.");
            else
            {
                QueueMode = (args[0].ToLower() == "-queue");
                ConfigFileName = args[1];
            }
            if (!File.Exists(ConfigFileName))
                throw new Exception("Config file not found: " + ConfigFileName);
            PlayPass PlayPass = new PlayPass();
            PlayPass.QueueMode = QueueMode;
            PlayPass.ProcessConfigFile(ConfigFileName);
        }
    }

}