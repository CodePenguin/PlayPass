using System;
using PlaySharp;

namespace PlayPass.Engine
{
    /// <summary>
    ///     An interface that validates if a media file can be queued.
    /// </summary>
    public interface IQueueValidator
    {
        void AddMediaToCounts(PlayOnVideo media);
        void AddMediaToQueueList(PlayOnVideo media);
        IDisposable AddTemporaryQueueLimits(PassQueueAction action);
        bool CanQueueMedia(PlayOnVideo media, out string message);
    }
}
