using System;
using System.IO;

namespace PlayPass
{
    /// <summary>
    ///     A class that parsers the command line and simplifies access to valid parameters
    /// </summary>
    internal class CommandLineParser
    {
        private string _configFileName = Path.Combine(Directory.GetCurrentDirectory(), "PlayPass.cfg");

        public CommandLineParser(string[] args)
        {
            Parse(args);
        }

        public string ConfigFileName
        {
            get { return _configFileName; }
        }

        public bool QueueMode { get; private set; }
        public bool SkipMode { get; private set; }
        public bool VerboseMode { get; private set; }

        private void Parse(string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i].ToLower();
                if (arg == "-queue")
                    QueueMode = true;
                else if (arg == "-skip")
                    SkipMode = true;
                else if (arg == "-verbose")
                    VerboseMode = true;
                else if (!arg.StartsWith("-") && i == args.Length - 1)
                    _configFileName = args[i];
                else
                    throw new ApplicationException(String.Format("Invalid command line: {0}", args[i]));
            }

            if (!File.Exists(ConfigFileName))
                throw new ApplicationException("Config file not found: " + ConfigFileName);
        }
    }
}