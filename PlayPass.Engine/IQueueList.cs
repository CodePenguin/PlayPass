using PlaySharp;

namespace PlayPass.Engine
{
    public interface IQueueList
    {
        /// <summary>
        ///     Initializes the Queue List with custom settings
        /// </summary>
        void Initialize(string connectionString);

        /// <summary>
        ///     Adds the supplied Media item to the list of already queued media.
        /// </summary>
        void AddMediaToList(PlayOnVideo media);

        /// <summary>
        ///     Indicates if a skip file exists for the supplied media item.
        /// </summary>
        bool MediaInList(PlayOnVideo media);
    }
}