using System;
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

        public QueueValidator(IQueueList queueList)
        {
            _queueList = queueList;
        }

        public void AddMediaToCounts(PlayOnVideo media)
        {
            _queuedCount++;
            _queuedDuration = _queuedDuration.Add(RunTimeToTimeSpan(media.RunTime));            
        }

        public void AddMediaToQueueList(PlayOnVideo media)
        {
            _queueList.AddMediaToList(media);
            AddMediaToCounts(media);
        }

        public bool CanQueueMedia(PlayOnVideo media, out string message)
        {
            var retValue = false;
            if (_queueList.MediaInList(media))
                message = "Already recorded or skipped.";
            else if (QueueCountLimit > 0 && QueueCountLimit <= _queuedCount)
                message = "Queue limit reached.";
            else if (QueueDurationLimit.Ticks > 0 && QueueDurationLimit < _queuedDuration.Add(RunTimeToTimeSpan(media.RunTime)))
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
