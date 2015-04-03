using System;
using PlaySharp;

namespace PlayPassEngine
{
    public class PlayPassProcessor
    {
        private readonly ILogManager _logManager;
        private readonly PlayOn _playOn;
        private readonly IQueueList _queueList;

        public bool QueueMode;
        public bool SkipMode;

        public PlayPassProcessor(PlayOn playOn, ILogManager logManager, IQueueList queueList)
        {
            _playOn = playOn;
            _logManager = logManager;
            _queueList = queueList;
        }

        /// <summary>
        ///     Individually processes the actions in all of the supplied passes
        /// </summary>
        public void ProcessPasses(PassItems passes)
        {
            foreach (var pass in passes)
                ProcessPass(pass);
        }

        /// <summary>
        ///     Processes all of the actions in a pass
        /// </summary>
        public void ProcessPass(PassItem pass)
        {
            PlayOnItem currentItem = _playOn.GetCatalog();
            if (!pass.Enabled)
                _logManager.Log("Skipping \"{0}\".", pass.Description);
            else
            {
                _logManager.Log("Processing \"{0}\"...", pass.Description);
                try
                {
                    ProcessActions(currentItem, pass.Actions);
                }
                catch (Exception ex)
                {
                    _logManager.Log("Error processing pass: " + ex.Message);
                }
            }
        }

        /// <summary>
        ///     Processes all of the actions against the currently selected item
        /// </summary>
        private void ProcessActions(PlayOnItem currentItem, PassActions actions)
        {
            if (!(currentItem is PlayOnFolder))
                return;
            foreach (var action in actions)
                ProcessAction(currentItem, action);
        }

        /// <summary>
        ///     Processes the current action against the currently selected item
        /// </summary>
        private void ProcessAction(PlayOnItem currentItem, PassAction action)
        {
            var matchPattern = action.Name;
            var foundItem = false;
            _logManager.Log("Matching \"{0}\"...", matchPattern);
            foreach (var childItem in ((PlayOnFolder)currentItem).Items)
            {
                _logManager.LogVerbose("Checking \"{0}\"...", childItem.Name);
                if (!Util.MatchesPattern(childItem.Name, matchPattern))
                    continue;
                foundItem = true;
                switch (action.Type)
                {
                    case PassActionType.Scan:
                        if (!(childItem is PlayOnFolder))
                            continue;
                        _logManager.Log("Entering \"{0}\"", childItem.Name);
                        ProcessActions(childItem, action.Actions);
                        _logManager.Log("Leaving \"{0}\"", childItem.Name);
                        break;

                    case PassActionType.Queue:
                        if (!(childItem is PlayOnVideo)) 
                            continue;
                        _logManager.Log("Queuing \"{0}\"...", childItem.Name);
                        QueueMedia((PlayOnVideo) childItem);
                        break;
                }
            }
            if (!foundItem)
                _logManager.Log("No matches \"{0}\".", matchPattern);              
        }

        /// <summary>
        ///     Checks to see if the item has already been recorded.  If not, it will queue the video for recording in PlayLater.
        /// </summary>
        private void QueueMedia(PlayOnVideo item)
        {
            var success = false;
            var message = "";
            if (_queueList.MediaInList(item))
                message = "Already recorded or skipped.";
            else if (SkipMode)
            {
                message = "Manually skipped.";
                _queueList.AddMediaToList(item);
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
                        _queueList.AddMediaToList(item);
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }
            }
            _logManager.Log("{0}{1}", (success ? "Queued" : "Skipped"), (message == "" ? "" : ": " + message));
        }
    }
}