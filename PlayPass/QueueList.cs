using System;
using System.Data.Common;
using System.IO;
using System.Text.RegularExpressions;
using PlaySharp;

namespace PlayPass
{
    /// <summary>
    /// A class that keeps track of previously queued media so it is not continually downloaded.
    /// </summary>
    class QueueList
    {
        protected String _SkipFilePath;

        public QueueList(String ConnectionString)
        {
            DbConnectionStringBuilder parser = new DbConnectionStringBuilder();
            parser.ConnectionString = ConnectionString;
            if (parser.ContainsKey("Data Source"))
                _SkipFilePath = parser["Data Source"].ToString();
            else
            {
                string MediaStorageLocation = PlayOnSettings.GetMediaStorageLocation();
                if (MediaStorageLocation == "")
                    throw new Exception("Unable to find PlayLater's Media Storage Location");
                _SkipFilePath = MediaStorageLocation;
            }
            if (!Directory.Exists(_SkipFilePath))
                throw new Exception(String.Format("Queue List data path does not exists: {0}", _SkipFilePath));
        }

        /// <summary>
        /// Adds the supplied Media item to the list of already queued media.
        /// </summary>
        public void AddMediaToList(PlayOnVideo Media)
        {
            string SkipFileName = SkipFileNameFromMedia(Media);
            using (FileStream fs = new FileStream(SkipFileName, FileMode.CreateNew)) { };
        }

        /// <summary>
        /// Generates the skip filename for a media item.
        /// </summary>
        private string SkipFileNameFromMedia(PlayOnVideo Media)
        {
            string temp = String.Format("{0} - {1}.playpass.skip", Media.Series, Media.MediaTitle);
            Regex re = new Regex("[<>:\"/\\|?*]");
            temp = re.Replace(temp, "_").TrimStart(' ', '-');
            return Path.Combine(_SkipFilePath, temp);
        }

        /// <summary>
        /// Indicates if a skip file exists for the supplied media item.
        /// </summary>
        public bool MediaInList(PlayOnVideo Media)
        {
            return File.Exists(SkipFileNameFromMedia(Media));
        }

    }
}
