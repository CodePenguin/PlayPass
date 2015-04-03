using System;
using System.Xml;

namespace PlaySharp
{
    /// <summary>
    ///     Represents a playable video in PlayOn.
    /// </summary>
    public class PlayOnVideo : PlayOnItem
    {
        private string _airDate;
        private string _artUrlLarge;
        private string _description;
        private bool _loadedDetails;
        private string _mediaTitle;
        private string _mediaUrl;
        private string _playLaterName;
        private string _playLaterUrl;
        private TimeSpan _runTime;
        private string _series;

        public PlayOnVideo(PlayOn api)
            : base(api)
        {
            _loadedDetails = false;
        }

        /// <summary>
        ///     A relative PlayOn URL for the large image for this video item.
        /// </summary>
        public string ArtUrlLarge
        {
            get
            {
                LoadDetails();
                return _artUrlLarge;
            }
        }

        /// <summary>
        ///     The date string representing the date this video item originally aired.
        /// </summary>
        public string AirDate
        {
            get
            {
                LoadDetails();
                return _airDate;
            }
        }

        /// <summary>
        ///     The full description for this video item.
        /// </summary>
        public string Description
        {
            get
            {
                LoadDetails();
                return _description;
            }
        }

        /// <summary>
        ///     The full media title for this video item.
        /// </summary>
        public string MediaTitle
        {
            get
            {
                LoadDetails();
                return _mediaTitle;
            }
        }

        /// <summary>
        ///     A relative PlayOn URL for streaming the video of this item.
        /// </summary>
        public string MediaUrl
        {
            get
            {
                LoadDetails();
                return _mediaUrl;
            }
        }

        /// <summary>
        ///     The name of the PlayLater instance associated with the connected server.
        /// </summary>
        public string PlayLaterName
        {
            get
            {
                LoadDetails();
                return _playLaterName;
            }
        }

        /// <summary>
        ///     A relative PlayOn URL for queueing this video in PlayLater.
        /// </summary>
        public string PlayLaterUrl
        {
            get
            {
                LoadDetails();
                return _playLaterUrl;
            }
        }

        /// <summary>
        ///     The runtime of this video item.
        /// </summary>
        public TimeSpan RunTime
        {
            get
            {
                LoadDetails();
                return _runTime;
            }
        }

        /// <summary>
        ///     The series name associated with this video item.
        /// </summary>
        public string Series
        {
            get
            {
                LoadDetails();
                return _series;
            }
        }

        public override void LoadFromNode(XmlNode node)
        {
            base.LoadFromNode(node);

            if (node.HasChildNodes)
            {
                _artUrlLarge = Util.GetChildNodeAttributeValue(node, "media", "art");
                _airDate = Util.GetChildNodeAttributeValue(node, "date", "name");
                _description = Util.GetChildNodeAttributeValue(node, "description", "name");
                _mediaTitle = Util.GetChildNodeAttributeValue(node, "media_title", "name");
                _mediaUrl = Util.GetChildNodeAttributeValue(node, "media", "src");
                _playLaterName = Util.GetChildNodeAttributeValue(node, "media_playlater", "name");
                _playLaterUrl = Util.GetChildNodeAttributeValue(node, "media_playlater", "src");
                _runTime = TimeSpan.Parse(Util.GetChildNodeAttributeValue(node, "time", "name"));
                _series = Util.GetChildNodeAttributeValue(node, "series", "name");
            }
        }

        private void LoadDetails()
        {
            if (_loadedDetails)
                return;
            PlayOn.LoadItemDetails(this);
            _loadedDetails = true;
        }

        /// <summary>
        ///     Attempts to queue this video item in PlayLater and indicates the result.
        /// </summary>
        public QueueVideoResult AddToPlayLaterQueue()
        {
            if (PlayLaterUrl == "")
                return QueueVideoResult.PlayLaterNotFound;
            var doc = PlayOn.XmlRequest(PlayLaterUrl);
            var success = (Util.GetNodeInnerText(doc.SelectSingleNode("result/status")) == "true");
            var message = Util.GetNodeInnerText(doc.SelectSingleNode("result/msg"));
            if (success)
                return QueueVideoResult.Success;
            if (message.IndexOf("already", StringComparison.Ordinal) > -1)
                return QueueVideoResult.AlreadyInQueue;
            return QueueVideoResult.Failed;
        }
    }

    public enum QueueVideoResult
    {
        AlreadyInQueue,
        PlayLaterNotFound,
        Success,
        Failed
    };
}