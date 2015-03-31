using System;
using System.IO;

namespace PlayPass
{
    /// <summary>
    /// A class that parsers the command line and simplifies access to valid parameters
    /// </summary>
    class CommandLineParser
    {
        private string _ConfigFileName = Path.Combine(Directory.GetCurrentDirectory(), "PlayPass.cfg");
        private bool _QueueMode = false;
        private bool _SkipMode = false;
        private bool _VerboseMode = false;

        public CommandLineParser(string[] args)
        {
            Parse(args);
        }

        private void Parse(string[] args)
        {
            for (int i = 0; i < args.Length; i++ )
            {
                string arg = args[i].ToLower();
                if (arg == "-queue")
                    _QueueMode = true;
                else if (arg == "-skip")
                    _SkipMode = true;
                else if (arg == "-verbose")
                    _VerboseMode = true;
                else if (!arg.StartsWith("-") && i == args.Length - 1)
                    _ConfigFileName = args[i];
                else
                    throw new ApplicationException(String.Format("Invalid command line: {0}", args[i]));
            }
   
            if (!File.Exists(ConfigFileName))
                throw new ApplicationException("Config file not found: " + ConfigFileName);
        }

        public string ConfigFileName { get { return _ConfigFileName; } }
        public bool QueueMode { get { return _QueueMode; } }
        public bool SkipMode { get { return _SkipMode; } }
        public bool VerboseMode { get { return _VerboseMode; } }
    }

}
