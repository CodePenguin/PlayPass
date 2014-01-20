using System;
using System.IO;

namespace PlayPass
{

    class Program
    {
        public static void Main(string[] args)
        {
            string ConfigFileName;
            if (args.Length == 0)
                ConfigFileName = Path.Combine(Directory.GetCurrentDirectory(), "PlayPass.config");
            else if (args.Length > 1)
                throw new Exception("Invalid command line arguments");
            else
                ConfigFileName = args[0];
            PlayPass PlayPass = new PlayPass();
            PlayPass.ProcessConfigFile(ConfigFileName);
        }
    }

}