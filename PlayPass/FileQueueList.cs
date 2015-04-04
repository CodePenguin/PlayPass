using System;
using System.Data.Common;
using System.IO;
using System.Text.RegularExpressions;
using PlayPassEngine;
using PlaySharp;

namespace PlayPass
{
    /// <summary>
    ///     A class that keeps track of previously queued media so it is not continually downloaded.
    /// </summary>
    internal class FileQueueList : IQueueList
    {
        private string _skipFilePath;

        /// <summary>
        ///     Initializes the Queue List with custom settings
        /// </summary>
        public void Initialize(string connectionString)
        {
            var parser = new DbConnectionStringBuilder {ConnectionString = connectionString};
            if (parser.ContainsKey("Data Source"))
                _skipFilePath = parser["Data Source"].ToString();
            else
            {
                var mediaStorageLocation = PlayOnSettings.GetMediaStorageLocation();
                if (mediaStorageLocation == "")
                    throw new Exception("Unable to find PlayLater's Media Storage Location");
                _skipFilePath = mediaStorageLocation;
            }
            if (!Directory.Exists(_skipFilePath))
                throw new Exception(String.Format("Queue List data path does not exists: {0}", _skipFilePath));
        }

        /// <summary>
        ///     Adds the supplied Media item to the list of already queued media.
        /// </summary>
        public void AddMediaToList(PlayOnVideo media)
        {
            var skipFileName = SkipFileNameFromMedia(media);
            var fs = new FileStream(skipFileName, FileMode.CreateNew);
            fs.Dispose();
        }

        /// <summary>
        ///     Indicates if a skip file exists for the supplied media item.
        /// </summary>
        public bool MediaInList(PlayOnVideo media)
        {
            return File.Exists(SkipFileNameFromMedia(media));
        }

        public static void RegisterClass()
        {
            QueueListFactory.RegisterClass(typeof (FileQueueList));
        }

        /// <summary>
        ///     Generates the skip filename for a media item.
        /// </summary>
        private string SkipFileNameFromMedia(PlayOnVideo media)
        {
            var temp = String.Format("{0} - {1}.playpass.skip", media.Series, media.MediaTitle);
            var re = new Regex("[<>:\"/\\|?*]");
            temp = re.Replace(temp, "_").TrimStart(' ', '-');
            return Path.Combine(_skipFilePath, temp);
        }
    }
}