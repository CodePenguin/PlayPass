using System;
using System.Collections.Generic;
using PlaySharp;

namespace PlayPass.Engine.Extensions
{
    /// <summary>
    ///     A class that validates if a media file can be queued based on specific limits or requirements.
    /// </summary>
    public class QueueValidator : IQueueValidator
    {
        private readonly IQueueList _queueList;
        public int QueueCountLimit { private get; set; }
        public TimeSpan QueueDurationLimit { private get; set; }

        private int _queuedCount;
        private TimeSpan _queuedDuration;
        private int _temporaryQueueCountLimit;
        private int _temporaryQueuedCount;
        private TimeSpan _temporaryQueueDurationLimit;
        private TimeSpan _temporaryQueuedDuration;
        private bool _temporaryLimitsEnabled;

        public QueueValidator(IQueueList queueList)
        {
            _queueList = queueList;
        }

        public void AddMediaToCounts(PlayOnVideo media)
        {
            if (_temporaryLimitsEnabled)
            {
                _temporaryQueuedCount++;
                _temporaryQueuedDuration = _queuedDuration.Add(RunTimeToTimeSpan(media.RunTime));
            }
            else
            {
                _queuedCount++;
                _queuedDuration = _queuedDuration.Add(RunTimeToTimeSpan(media.RunTime));
            }
        }

        public void AddMediaToQueueList(PlayOnVideo media)
        {
            _queueList.AddMediaToList(media);
            AddMediaToCounts(media);
        }

        public IDisposable AddTemporaryQueueLimits(PassQueueAction action)
        {
            if (action.CountLimit > 0 || action.DurationLimit.Ticks > 0)
            {
                _temporaryQueueCountLimit = action.CountLimit;
                _temporaryQueueDurationLimit = action.DurationLimit;
                _temporaryLimitsEnabled = true;
            }
            return new DisposableActionObject(() =>
            {
                if (_temporaryLimitsEnabled)
                {
                    _queuedCount += _temporaryQueuedCount;
                    _queuedDuration = _queuedDuration.Add(_temporaryQueuedDuration);
                }
                _temporaryLimitsEnabled = false;
            });
        }

        public bool CanQueueMedia(PlayOnVideo media, out string message)
        {
            var queueCountLimit = (_temporaryLimitsEnabled ? _temporaryQueueCountLimit : QueueCountLimit);
            var queuedCount = (_temporaryLimitsEnabled ? _temporaryQueuedCount : _queuedCount);
            var queueDurationLimit = (_temporaryLimitsEnabled ? _temporaryQueueDurationLimit : QueueDurationLimit);
            var queuedDuration = (_temporaryLimitsEnabled ? _temporaryQueuedDuration : _queuedDuration);
            var retValue = false;
            if (_queueList.MediaInList(media))
                message = "Already recorded or skipped.";
            else if (queueCountLimit > 0 && queueCountLimit <= queuedCount)
                message = "Queue limit reached.";
            else if (queueDurationLimit.Ticks > 0 && queueDurationLimit < queuedDuration.Add(RunTimeToTimeSpan(media.RunTime)))
                message = "Queue duration limit reached.";
            else
            {
                message = "";
                retValue = true;
            }
            return retValue;
        }

        private static TimeSpan RunTimeToTimeSpan(string runTime)
        {
            TimeSpan result;
            if (!TimeSpan.TryParse(runTime, out result))
                result = new TimeSpan(5, 0, 0);
            return result;
        }
    }
}
