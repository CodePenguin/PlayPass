using System;
using System.Data.Common;
using System.IO;
using System.Text.RegularExpressions;
using PlaySharp;

namespace PlayPass
{
    /// <summary>
    ///     A class that keeps track of previously queued media so it is not continually downloaded.
    /// </summary>
    internal class QueueList
    {
        protected String SkipFilePath;

        /// <summary>
        ///     Initializes the QueueList
        /// </summary>
        public QueueList(String connectionString)
        {
            var parser = new DbConnectionStringBuilder() {ConnectionString = connectionString};
            if (parser.ContainsKey("Data Source"))
                SkipFilePath = parser["Data Source"].ToString();
            else
            {
                var mediaStorageLocation = PlayOnSettings.GetMediaStorageLocation();
                if (mediaStorageLocation == "")
                    throw new Exception("Unable to find PlayLater's Media Storage Location");
                SkipFilePath = mediaStorageLocation;
            }
            if (!Directory.Exists(SkipFilePath))
                throw new Exception(String.Format("Queue List data path does not exists: {0}", SkipFilePath));
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
        ///     Generates the skip filename for a media item.
        /// </summary>
        private string SkipFileNameFromMedia(PlayOnVideo media)
        {
            var temp = String.Format("{0} - {1}.playpass.skip", media.Series, media.MediaTitle);
            var re = new Regex("[<>:\"/\\|?*]");
            temp = re.Replace(temp, "_").TrimStart(' ', '-');
            return Path.Combine(SkipFilePath, temp);
        }

        /// <summary>
        ///     Indicates if a skip file exists for the supplied media item.
        /// </summary>
        public bool MediaInList(PlayOnVideo media)
        {
            return File.Exists(SkipFileNameFromMedia(media));
        }
    }
}