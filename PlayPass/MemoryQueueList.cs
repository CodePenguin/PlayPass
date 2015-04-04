using System.Collections.Generic;
using PlayPass.Engine;
using PlaySharp;

namespace PlayPass
{
    internal class MemoryQueueList : IQueueList
    {
        private readonly List<string> _list = new List<string>();

        /// <summary>
        ///     Initializes the Queue List with custom settings
        /// </summary>
        public void Initialize(string connectionString)
        {
            // Do nothing
        }

        /// <summary>
        ///     Adds the supplied Media item to the list of already queued media.
        /// </summary>
        public void AddMediaToList(PlayOnVideo media)
        {
            if (MediaInList(media))
                return;
            _list.Add(KeyFromMedia(media));
        }

        /// <summary>
        ///     Indicates if a skip file exists for the supplied media item.
        /// </summary>
        public bool MediaInList(PlayOnVideo media)
        {
            return _list.Contains(KeyFromMedia(media));
        }

        /// <summary>
        ///     Registers this class with the QueueListFactory
        /// </summary>
        public static void RegisterClass()
        {
            QueueListFactory.RegisterClass(typeof (MemoryQueueList));
        }

        /// <summary>
        ///     Generates the skip filename for a media item.
        /// </summary>
        private static string KeyFromMedia(PlayOnVideo media)
        {
            return media.Series + " - " + media.MediaTitle;
        }
    }
}