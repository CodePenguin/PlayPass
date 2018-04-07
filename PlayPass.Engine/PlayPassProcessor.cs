using System;
using PlaySharp;

namespace PlayPass.Engine
{
    /// <summary>
    ///     The core processing engine for PlayPass that performs the actions associated with passes.
    /// </summary>
    public class PlayPassProcessor
    {
        private readonly ILogManager _logManager;
        private readonly IPlayOn _playOn;
        private readonly IQueueValidator _queueValidator;
        public bool QueueMode;
        public bool SkipMode;

        public PlayPassProcessor(IPlayOn playOn, ILogManager logManager, IQueueValidator queueValidator)
        {
            _playOn = playOn;
            _logManager = logManager;
            _queueValidator = queueValidator;
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
                _logManager.Log("Skipping pass \"{0}\".", pass.Description);
            else
            {
                _logManager.Log("Processing pass \"{0}\"...", pass.Description);
                try
                {
                    ProcessActions(currentItem, pass.Actions);
                }
                catch (Exception ex)
                {
                    _logManager.LogException(ex);
                }
                _logManager.Log("Finished processing pass \"{0}\"...", pass.Description);
            }
        }

        /// <summary>
        ///     Processes all of the actions against the currently selected item
        /// </summary>
        private void ProcessActions(PlayOnItem currentItem, PassActions actions)
        {
            var folder = (currentItem as PlayOnFolder);
            if (folder == null) return;
            foreach (var action in actions)
            {
                using (_logManager.NextLogVerboseDepth())
                {
                    switch (action.Type)
                    {
                        case PassActionType.Search:
                            ProcessSearchAction(folder, action);
                            break;
                        case PassActionType.Queue:
                        case PassActionType.Scan:
                            ProcessMatchAction(folder, action);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Processes a matching action against the items in a folder.
        /// </summary>
        private void ProcessMatchAction(PlayOnFolder folder, PassAction action)
        {
            var matchPattern = action.Name;
            var excludePattern = action.Exclude;
            var foundItem = false;
            _logManager.Log("Matching \"{0}\"...", matchPattern);
            using (_logManager.NextLogDepth())
            {
                foreach (var childItem in folder.Items)
                {
                    _logManager.LogVerbose("Checking \"{0}\"...", childItem.Name);
                    if (!Util.MatchesPattern(childItem.Name, matchPattern))
                        continue;
                    if (Util.MatchesPattern(childItem.Name, excludePattern))
                    {
                        _logManager.LogVerbose("Excluded match.");
                        continue;
                    }
                    foundItem = true;
                    switch (action.Type)
                    {
                        case PassActionType.Scan:
                            if (!(childItem is PlayOnFolder))
                            {
                                _logManager.LogVerbose("\"{0}\" is not a folder.", childItem.Name);
                                return;
                            }
                            using (_logManager.NextLogVerboseDepth())
                            {
                                _logManager.LogVerbose("Entering \"{0}\"", childItem.Name);
                                ProcessActions(childItem, action.Actions);
                                _logManager.LogVerbose("Leaving \"{0}\"", childItem.Name);
                            }
                            break;

                        case PassActionType.Queue:
                            if (!(childItem is PlayOnVideo))
                            {
                                _logManager.LogVerbose("\"{0}\" is not a video.", childItem.Name);
                                return;
                            }
                            _logManager.Log("Queuing \"{0}\"...", childItem.Name);
                            using (_logManager.NextLogDepth())
                                QueueMedia((PlayOnVideo) childItem);
                            break;
                    }
                }
                if (!foundItem)
                    _logManager.Log("No matches for \"{0}\".", matchPattern);
            }
        }

        /// <summary>
        ///     Processes a search action against a folder.
        /// </summary>
        private void ProcessSearchAction(PlayOnFolder folder, PassAction action)
        {
            if (!folder.Searchable)
            {
                _logManager.Log("\"{0}\" is not searchable.", folder.Name);
                return;
            }
            var searchTerm = action.Name;
            _logManager.Log("Searching for \"{0}\"...", searchTerm);
            using (_logManager.NextLogDepth())
            {
                var searchResults = folder.Search(searchTerm);
                if (searchResults.Items.Count == 0)
                {
                    _logManager.Log("No matches for \"{0}\".", searchTerm);
                    return;
                }
                ProcessActions(searchResults, action.Actions);
            }
        }

        /// <summary>
        ///     Checks to see if the item has already been recorded.  If not, it will queue the video for recording in PlayLater.
        /// </summary>
        private void QueueMedia(PlayOnVideo media)
        {
            var success = false;
            string message;
            if (SkipMode)
            {
                message = "Manually skipped.";
                _queueValidator.AddMediaToQueueList(media);
            }
            else if (_queueValidator.CanQueueMedia(media, out message))
            {
                if (!QueueMode)
                {
                    success = true;
                    message = "Pending queue mode.";
                    _queueValidator.AddMediaToCounts(media);
                }
                else
                {
                    try
                    {
                        var queueResult = _playOn.QueueMedia(media);
                        if (queueResult == PlayOnConstants.QueueVideoResult.PlayLaterNotFound)
                            message = "PlayLater queue link not found. PlayLater may not be running.";
                        else if (queueResult == PlayOnConstants.QueueVideoResult.AlreadyInQueue)
                            message = "Already queued.";
                        success = (queueResult == PlayOnConstants.QueueVideoResult.Success);
                        if (success)
                            _queueValidator.AddMediaToQueueList(media);
                    }
                    catch (Exception ex)
                    {
                        message = ex.Message;
                    }
                }
            }
            _logManager.Log("{0}{1}", (success ? "Queued" : "Skipped"), (message == "" ? "" : ": " + message));
        }
    }
}