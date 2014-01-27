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

            try
            {

                Console.WriteLine("PlayPass Auto Queueing Engine\n");

                if (args.Length == 0)
                    ConfigFileName = Path.Combine(Directory.GetCurrentDirectory(), "PlayPass.cfg");
                else if (args.Length == 1)
                {
                    if (args[0].ToLower() == "-queue")
                    {
                        QueueMode = true;
                        ConfigFileName = Path.Combine(Directory.GetCurrentDirectory(), "PlayPass.cfg");
                    }
                    else
                        ConfigFileName = args[0];
                }
                else if ((args.Length == 2) && (args[0].ToLower() == "-queue"))
                {
                    QueueMode = true;
                    ConfigFileName = args[1];
                }
                else
                    throw new Exception("Invalid command line arguments.");
                if (!File.Exists(ConfigFileName))
                    throw new Exception("Config file not found: " + ConfigFileName);
                PlayPass PlayPass = new PlayPass();
                PlayPass.QueueMode = QueueMode;
                PlayPass.ProcessConfigFile(ConfigFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("The following exception has occurred:\n   " + ex.Message);
            }
        }
    }

}