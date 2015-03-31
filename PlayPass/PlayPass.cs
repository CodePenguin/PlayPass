using System;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
using PlaySharp;

namespace PlayPass
{

    class PlayPass
    {
        string ServerHost = PlayOnConstants.DefaultHost;
        int ServerPort = PlayOnConstants.DefaultPort;
        public bool QueueMode = false;
        public bool SkipMode = false;
        public bool VerboseMode = false;

        /// <summary>
        /// Processes the config file by loading extra settings and then executing the ProcessPass procedure on each pass node.
        /// </summary>
        public void ProcessConfigFile(string FileName)
        {
            XmlDocument Config = new XmlDocument();
            Config.Load(FileName);
            XmlNode SettingsNode = Config.SelectSingleNode("playpass/settings");
            string QueueListConnectionString = "";
            if (SettingsNode != null)
            {
                ServerHost = PlaySharp.Util.GetNodeAttributeValue(SettingsNode, "server", ServerHost);
                ServerPort = int.Parse(PlaySharp.Util.GetNodeAttributeValue(SettingsNode, "port", ServerPort.ToString()));
                QueueListConnectionString = PlaySharp.Util.GetNodeAttributeValue(SettingsNode, "queuelist", "");
            }

            QueueList QueueList = new QueueList(QueueListConnectionString);

			WriteVerboseLog("Connecting to {0}:{1}...", ServerHost, ServerPort);
            PlayOn PlayOn = new PlayOn(ServerHost, ServerPort);

            XmlNode PassesNode = Config.SelectSingleNode("playpass/passes");
            if (PassesNode == null)
                throw new ApplicationException("A passes node was not found in the config file");
            foreach (XmlNode PassNode in PassesNode.SelectNodes("pass"))
                ProcessPass(PlayOn, QueueList, PassNode);
        }

        /// <summary>
        /// Executes the search and queue function on a pass node in the config file.
        /// </summary>
        /// <param name="PassNode">A pass node from the config file.</param>
        void ProcessPass(PlayOn PlayOn, QueueList QueueList, XmlNode PassNode)
        {
            PlayOnItem CurrItem = PlayOn.GetCatalog();
            if (Util.GetNodeAttributeValue(PassNode, "enabled", "0") == "0")
				WriteLog("Skipping \"{0}\".", Util.GetNodeAttributeValue(PassNode, "description"));
			else
            {
                WriteLog("Processing \"{0}\"...", Util.GetNodeAttributeValue(PassNode, "description"));
                try
                {
                    foreach (XmlNode Node in PassNode.ChildNodes)
                    {
                        string MatchPattern = Util.GetNodeAttributeValue(Node, "name");
                        bool FoundItem = false;
                        if (!(CurrItem is PlayOnFolder))
                            continue;
                        if (Node.Name == "scan")
                        {
                            WriteLog("  Matching \"{0}\"...", MatchPattern);
                            foreach (PlayOnItem ChildItem in ((PlayOnFolder)CurrItem).Items)
                            {
                                if (ChildItem is PlayOnFolder)
                                {
                                    WriteVerboseLog("    Checking \"{0}\"...", ChildItem.Name);
                                    if (Util.MatchesPattern(ChildItem.Name, MatchPattern))
                                    {
                                        WriteLog("    Scanning \"{0}\"", ChildItem.Name);
                                        FoundItem = true;
                                        CurrItem = ChildItem;
                                        break;
                                    }
                                }
                            }
                            if (!FoundItem)
                                WriteLog("    No matches \"{0}\".", MatchPattern);
                        }
                        else if (Node.Name == "queue")
                        {
                            WriteLog("  Matching \"{0}\"...", MatchPattern);
                            foreach (PlayOnItem ChildItem in ((PlayOnFolder)CurrItem).Items)
                            {
                                if (ChildItem is PlayOnVideo)
                                {
                                    WriteVerboseLog("    Checking \"{0}\"...", ChildItem.Name);
                                    if (Util.MatchesPattern(ChildItem.Name, MatchPattern))
                                    {
                                        WriteLog("    Queuing \"{0}\"", ChildItem.Name);
                                        QueueMedia(QueueList, (PlayOnVideo)ChildItem);
                                        FoundItem = true;
                                    }
                                }
                            }
                            if (!FoundItem)
                                WriteLog("    No matches \"{0}\".", MatchPattern);
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteLog("Error processing pass: " + ex.Message.ToString());
                }
            }
        }

        /// <summary>
        /// Checks the queue list to see if the item has already been recorded.  If not, it will queue the video for record in PlayLater.
        /// </summary>
        void QueueMedia(QueueList QueueList, PlayOnVideo Item)
        {
            bool Success = false;
            string Message = "";
            if (QueueList.MediaInList(Item))
                Message = "Already recorded or skipped.";
            else if (SkipMode)
            {
                Success = false;
                Message = "Manually skipped.";
                QueueList.AddMediaToList(Item);
            }
            else if (!QueueMode)
            {
                Success = true;
                Message = "Video will be queued on next run in Queue Mode.";
            }
            else
            {
                try
                {
                    QueueVideoResult QueueResult = Item.AddToPlayLaterQueue();
                    if (QueueResult == QueueVideoResult.PlayLaterNotFound)
                        Message = "PlayLater queue link not found. PlayLater may not be running.";
                    else if (QueueResult == QueueVideoResult.AlreadyInQueue)
                        Message = "Already queued.";
                    Success = (QueueResult == QueueVideoResult.Success);
                    if (Success)
                        QueueList.AddMediaToList(Item);
                }
                catch (Exception ex)
                {
                    Message = ex.Message.ToString();
                }
            }
            WriteLog("      {0}{1}", (Success ? "Queued" : "Skipped"), (Message == "" ? "" : ": " + Message));
        }
		
		/// <summary>
        /// The PlayPass version number
        /// </summary>
        public string Version
        {
			get
			{
				Assembly assembly = Assembly.GetExecutingAssembly();
				FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
				return fileVersionInfo.ProductVersion;
			}
        }

        /// <summary>
        /// Writes a log message to the console and to the debug area.
        /// </summary>
        void WriteLog(string Message)
        {
            Console.WriteLine(Message);
            Debug.WriteLine(Message);
        }

        /// <summary>
        /// Writes a log message to the console and to the debug area.
        /// </summary>
        void WriteLog(string Message, params object[] args)
        {
            Message = String.Format(Message, args);
            Console.WriteLine(Message);
            Debug.WriteLine(Message);
        }

        /// <summary>
        /// Writes a log message to the console and to the debug area.
        /// </summary>
        void WriteVerboseLog(string Message, params object[] args)
        {
			if (!VerboseMode)
				return;
            Message = String.Format(Message, args);
            Console.WriteLine(Message);
            Debug.WriteLine(Message);
        }
    }

}
