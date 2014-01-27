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
        public bool QueueMode = true;

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
            if (Util.GetNodeAttributeValue(PassNode, "enabled", "0") == "1")
            {
                WriteLog(String.Format("Processing {0}...", Util.GetNodeAttributeValue(PassNode, "description")));
                try
                {
                    List<string> Paths = new List<string>();
                    foreach (XmlNode ScanNode in PassNode.SelectNodes("scan"))
                        if (ScanNode.Attributes["name"] != null)
                            Paths.Add(ScanNode.Attributes["name"].Value);
                    if (Paths.Count > 0)
                        ProcessPaths(PlayOn, PlayOnConstants.DefaultURL, Paths);
                }
                catch (Exception ex)
                {
                    WriteLog("Error processing pass: " + ex.Message.ToString());
                }
            }
        }

        /// <summary>
        /// Recursively walks through the items in Paths and attempts to find the next text item at the current position in the PlayOn tree.
        /// </summary>
        /// <param name="URL">A relative PlayOn URL indicating the current position in the PlayOn tree.</param>
        /// <param name="Paths">A list of text items to scan through.</param>
        void ProcessPaths(PlayOn PlayOn, string URL, List<string> Paths)
        {
            bool FoundItem = false;
            PlayOnItem Item = PlayOn.GetItem(URL);
            if (Paths.Count > 0)
            {
                string MatchPattern = Paths[0];
                Paths.RemoveAt(0);
                if (Item is PlayOnFolder)
                {
                    WriteLog("  Looking for: " + MatchPattern);
                    foreach (PlayOnItem ChildItem in ((PlayOnFolder)Item).Items)
                    {
                        if ((ChildItem is PlayOnFolder || ChildItem is PlayOnVideo) && ChildItem.Name == MatchPattern)
                        {
                            WriteLog("    Found: " + ChildItem.Name);
                            ProcessPaths(PlayOn, ChildItem.URL, Paths);
                            FoundItem = true;
                        }
                    }
                    if (!FoundItem)
                        WriteLog("    No item was found for the text \"" + MatchPattern + "\".");
                }
            }
            else if (Item is PlayOnVideo)
                QueueMedia((PlayOnVideo)Item);
            else if (Item is PlayOnFolder)
            {
                foreach (PlayOnItem ChildItem in ((PlayOnFolder)Item).Items)
                    if (ChildItem is PlayOnVideo)
                    {
                        QueueMedia((PlayOnVideo)ChildItem);
                        FoundItem = true;
                    }
                if (!FoundItem)
                    WriteLog("      No Video items were found at this level.");
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
            FileName = re.Replace(FileName, "_");
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
            WriteLog("        QueueVideo Response: " + (Success ? "Success" : "Failure") + (Message == "" ? "" : " - " + Message));
        }

        /// <summary>
        /// Writes a log message to the console and to the debug area.
        /// </summary>
        void WriteLog(string Message)
        {
            Console.WriteLine(Message);
            Debug.WriteLine(Message);
        }
    }

}
