using System;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
using PlaySharp;

namespace PlayPass
{
    internal class PlayPass
    {
        private readonly LogManager _logManager;
        private string _serverHost = PlayOnConstants.DefaultHost;
        private int _serverPort = PlayOnConstants.DefaultPort;
        public bool QueueMode = false;
        public bool SkipMode = false;
        public bool VerboseMode = false;

        public PlayPass(LogManager logManager)
        {
            _logManager = logManager;
        }

        /// <summary>
        ///     Processes the config file by loading extra settings and then executing the ProcessPass procedure on each pass node.
        /// </summary>
        public void ProcessConfigFile(string fileName)
        {
            var config = new XmlDocument();
            config.Load(fileName);
            var settingsNode = config.SelectSingleNode("playpass/settings");
            var queueListConnectionString = "";
            if (settingsNode != null)
            {
                _serverHost = Util.GetNodeAttributeValue(settingsNode, "server", _serverHost);
                _serverPort = int.Parse(Util.GetNodeAttributeValue(settingsNode, "port", _serverPort.ToString()));
                queueListConnectionString = Util.GetNodeAttributeValue(settingsNode, "queuelist", "");
            }

            var queueList = new QueueList(queueListConnectionString);

            _logManager.LogVerbose("Connecting to {0}:{1}...", _serverHost, _serverPort);
            var playOn = new PlayOn(_serverHost, _serverPort);

            var passesNode = config.SelectSingleNode("playpass/passes");
            if (passesNode == null)
                throw new ApplicationException("A passes node was not found in the config file");
            var passNodes = passesNode.SelectNodes("pass");
            if (passNodes == null)
                return;
            foreach (XmlNode passNode in passNodes)
                ProcessPass(playOn, queueList, passNode);
        }

        /// <summary>
        ///     Executes the search and queue function on a pass node in the config file.
        /// </summary>
        /// <param name="playOn">PlayOn API instance</param>
        /// <param name="queueList">QueueList provider for skipping previously queued items</param>
        /// <param name="passNode">A pass node from the config file.</param>
        private void ProcessPass(PlayOn playOn, QueueList queueList, XmlNode passNode)
        {
            PlayOnItem currItem = playOn.GetCatalog();
            if (Util.GetNodeAttributeValue(passNode, "enabled", "0") == "0")
                _logManager.Log("Skipping \"{0}\".", Util.GetNodeAttributeValue(passNode, "description"));
            else
            {
                _logManager.Log("Processing \"{0}\"...", Util.GetNodeAttributeValue(passNode, "description"));
                try
                {
                    foreach (XmlNode node in passNode.ChildNodes)
                    {
                        var matchPattern = Util.GetNodeAttributeValue(node, "name");
                        var foundItem = false;
                        if (!(currItem is PlayOnFolder))
                            continue;
                        if (node.Name == "scan")
                        {
                            _logManager.Log("  Matching \"{0}\"...", matchPattern);
                            foreach (var childItem in ((PlayOnFolder) currItem).Items)
                            {
                                if (!(childItem is PlayOnFolder))
                                    continue;
                                _logManager.LogVerbose("    Checking \"{0}\"...", childItem.Name);
                                if (!Util.MatchesPattern(childItem.Name, matchPattern))
                                    continue;
                                _logManager.Log("    Scanning \"{0}\"", childItem.Name);
                                foundItem = true;
                                currItem = childItem;
                                break;
                            }
                            if (!foundItem)
                                _logManager.Log("    No matches \"{0}\".", matchPattern);
                        }
                        else if (node.Name == "queue")
                        {
                            _logManager.Log("  Matching \"{0}\"...", matchPattern);
                            foreach (var childItem in ((PlayOnFolder) currItem).Items)
                            {
                                if (!(childItem is PlayOnVideo)) continue;
                                _logManager.LogVerbose("    Checking \"{0}\"...", childItem.Name);
                                if (!Util.MatchesPattern(childItem.Name, matchPattern))
                                    continue;
                                _logManager.Log("    Queuing \"{0}\"", childItem.Name);
                                QueueMedia(queueList, (PlayOnVideo) childItem);
                                foundItem = true;
                            }
                            if (!foundItem)
                                _logManager.Log("    No matches \"{0}\".", matchPattern);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logManager.Log("Error processing pass: " + ex.Message);
                }
            }
        }

        /// <summary>
        ///     Checks the queue list to see if the item has already been recorded.  If not, it will queue the video for record in
        ///     PlayLater.
        /// </summary>
        private void QueueMedia(QueueList queueList, PlayOnVideo item)
        {
            var success = false;
            var message = "";
            if (queueList.MediaInList(item))
                message = "Already recorded or skipped.";
            else if (SkipMode)
            {
                message = "Manually skipped.";
                queueList.AddMediaToList(item);
            }
            else if (!QueueMode)
            {
                success = true;
                message = "Video will be queued on next run in Queue Mode.";
            }
            else
            {
                try
                {
                    var queueResult = item.AddToPlayLaterQueue();
                    if (queueResult == QueueVideoResult.PlayLaterNotFound)
                        message = "PlayLater queue link not found. PlayLater may not be running.";
                    else if (queueResult == QueueVideoResult.AlreadyInQueue)
                        message = "Already queued.";
                    success = (queueResult == QueueVideoResult.Success);
                    if (success)
                        queueList.AddMediaToList(item);
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }
            }
            _logManager.Log("      {0}{1}", (success ? "Queued" : "Skipped"), (message == "" ? "" : ": " + message));
        }
    }
}