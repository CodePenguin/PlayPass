using System;
using System.IO;

namespace PlayPass
{

    class Program
    {
        public static void Main(string[] args)
        {
            string ConfigFileName;
            PlayPass PlayPass = new PlayPass();

            try
            {
                Console.WriteLine("PlayPass Auto Queueing Engine\n");
                ConfigFileName = Path.Combine(Directory.GetCurrentDirectory(), "PlayPass.cfg");
                if (args.Length == 1)
                {
                    if (args[0].ToLower() == "-queue")
                        PlayPass.QueueMode = true;
                    else if (args[0].ToLower() == "-verbose")
                        PlayPass.VerboseMode = true;
                    else
                        ConfigFileName = args[0];
                }
                else if ((args.Length == 2) && (args[0].ToLower() == "-queue" || args[0].ToLower() == "-verbose"))
                {
                    if (args[0].ToLower() == "-queue")
                        PlayPass.QueueMode = true;
                    else if (args[0].ToLower() == "-verbose")
                        PlayPass.VerboseMode = true;
                    ConfigFileName = args[1];
                }
                else if (args.Length > 0)
                    throw new Exception("Invalid command line arguments.");
                if (!File.Exists(ConfigFileName))
                    throw new Exception("Config file not found: " + ConfigFileName);
                
                PlayPass.ProcessConfigFile(ConfigFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("The following exception has occurred:\n   " + ex.Message);
            }
        }
    }

}