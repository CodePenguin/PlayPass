using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using PlaySharp;

namespace PlayPass
{

    class PlayPass
    {
        string ServerHost = PlayOnConstants.DefaultHost;
        int ServerPort = PlayOnConstants.DefaultPort;
        string MediaStorageLocation = "";
        string MediaFileExt = "";
        public bool QueueMode = false;
        public bool VerboseMode = false;

        /// <summary>
        /// Loads the PlayOn settings from the local computer's registry.
        /// </summary>
        void LoadPlayOnSettings()
        {
            MediaStorageLocation = PlayOnSettings.GetMediaStorageLocation();
            if (MediaStorageLocation == "")
                throw new Exception("Unable to find PlayLater's Media Storage Location");
            MediaFileExt = PlayOnSettings.GetPlayLaterVideoFormat();
        }

        /// <summary>
        /// Processes the config file by loading extra settings and then executing the ProcessPass procedure on each pass node.
        /// </summary>
        /// <param name="FileName"></param>
        public void ProcessConfigFile(string FileName)
        {
            LoadPlayOnSettings();
            try
            {
                XmlDocument Config = new XmlDocument();
                Config.Load(FileName);
                XmlNode SettingsNode = Config.SelectSingleNode("playpass/settings");
                if (SettingsNode != null)
                {
                    ServerHost = PlaySharp.Util.GetNodeAttributeValue(SettingsNode, "server", ServerHost);
                    ServerPort = int.Parse(PlaySharp.Util.GetNodeAttributeValue(SettingsNode, "port", ServerPort.ToString()));
                }

                PlayOn PlayOn = new PlayOn(ServerHost, ServerPort);

                XmlNode PassesNode = Config.SelectSingleNode("playpass/passes");
                if (PassesNode == null)
                    throw new Exception("A passes node was found in the config file");
                foreach (XmlNode PassNode in PassesNode.SelectNodes("pass"))
                    ProcessPass(PlayOn, PassNode);
            }
            catch (Exception ex)
            {
                WriteLog("Error processing config file: " + ex.Message.ToString());
            }
        }

        /// <summary>
        /// Executes the search and queue function on a pass node in the config file.
        /// </summary>
        /// <param name="PassNode">A pass node from the config file.</param>
        void ProcessPass(PlayOn PlayOn, XmlNode PassNode)
        {
            PlayOnItem CurrItem = PlayOn.GetCatalog();
            if (Util.GetNodeAttributeValue(PassNode, "enabled", "0") == "1")
            {
                WriteLog("Processing {0}...", Util.GetNodeAttributeValue(PassNode, "description"));
                try
                {
                    List<string> Paths = new List<string>();
                    foreach (XmlNode Node in PassNode.ChildNodes)
                    {
                        string MatchPattern = Util.GetNodeAttributeValue(Node, "name");
                        bool FoundItem = false;
                        if (!(CurrItem is PlayOnFolder))
                            continue;
                        if (Node.Name == "scan")
                        {
                            WriteLog("  Looking for a folder with text matching \"{0}\"...", MatchPattern);
                            foreach (PlayOnItem ChildItem in ((PlayOnFolder)CurrItem).Items)
                            {
                                if (ChildItem is PlayOnFolder)
                                {
                                    if (VerboseMode)
                                        WriteLog("    Checking pattern against \"{0}\"...", ChildItem.Name);
                                    if (Util.MatchesPattern(ChildItem.Name, MatchPattern))
                                    {
                                        WriteLog("    Found: " + ChildItem.Name);
                                        FoundItem = true;
                                        CurrItem = ChildItem;
                                        break;
                                    }
                                }
                            }
                            if (!FoundItem)
                                WriteLog("    No folders were found text matching \"{0}\".", MatchPattern);
                        }
                        else if (Node.Name == "queue")
                        {
                            WriteLog("  Looking for videos with text matching \"{0}\"...", MatchPattern);
                            foreach (PlayOnItem ChildItem in ((PlayOnFolder)CurrItem).Items)
                            {
                                if (ChildItem is PlayOnVideo)
                                {
                                    if (VerboseMode)
                                        WriteLog("    Checking pattern against \"{0}\"...", ChildItem.Name);
                                    if (Util.MatchesPattern(ChildItem.Name, MatchPattern))
                                    {
                                        WriteLog("    Found: {0}", ChildItem.Name);
                                        QueueMedia((PlayOnVideo)ChildItem);
                                        FoundItem = true;
                                    }
                                }
                            }
                            if (!FoundItem)
                                WriteLog("    No videos were found that matched this criteria.");
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
        /// Checks the local file system to see if the item has already been recorded.  If not, it will queue the video for record in PlayLater.
        /// </summary>
        void QueueMedia(PlayOnVideo Item)
        {
            bool Success = false;
            string Message = "";

            WriteLog("      Adding Video to Queue: " + Item.Name);
            string FileName = String.Format("{0} - {1}{2}", Item.Series, Item.MediaTitle, MediaFileExt);
            Regex re = new Regex("[<>:\"/\\|?*]");
            FileName = re.Replace(FileName, "_").TrimStart(' ','-');
            if (File.Exists(Path.Combine(MediaStorageLocation, FileName)))
                Message = String.Format("Video already recorded to {0}.", Path.Combine(MediaStorageLocation, FileName));
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
                        Message = "The requested media item is already in the queue.";
                    Success = (QueueResult == QueueVideoResult.Success);
                }
                catch (Exception ex)
                {
                    Message = ex.Message.ToString();
                }
            }
            WriteLog("        QueueVideo Response: {0}{1}",(Success ? "Success" : "Skipped"), (Message == "" ? "" : " - " + Message));
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
    }

}
