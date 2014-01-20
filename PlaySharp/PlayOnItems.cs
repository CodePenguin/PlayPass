using System;
using System.Collections.Generic;
using System.Xml;

namespace PlaySharp
{

    /// <summary>
    /// The base class that all PlayOn items inherit from.
    /// </summary>
    public class PlayOnItem
    {
        protected PlayOn _API;
        protected string _ArtURL;
        protected string _Name;
        protected string _URL;

        public PlayOnItem(PlayOn API)
        {
            _API = API;
        }

        public virtual void LoadFromNode(XmlNode Node)
        {
            _Name = Util.GetNodeAttributeValue(Node, "name");
            _URL = Util.GetNodeAttributeValue(Node, "href");
            _ArtURL = Util.GetNodeAttributeValue(Node, "art");
        }

        /// <summary>
        /// A relative PlayOn URL for a small image of the item.
        /// </summary>
        public string ArtURL { get {return _ArtURL;} }

        /// <summary>
        /// The display name of the PlayOn item.
        /// </summary>
        public string Name { get { return _Name; } }

        /// <summary>
        /// A relative PlayOn URL for the details of this item.
        /// </summary>
        public string URL { get { return _URL; } }
    }

    /// <summary>
    /// Represents a folder in PlayOn.  The child items can be accessed by using the Items property.
    /// </summary>
    public class PlayOnFolder : PlayOnItem
    {
        protected List<PlayOnItem> _Items = null;

        public PlayOnFolder(PlayOn API) 
            : base(API)
        {
        }

        /// <summary>
        /// A list containing all child PlayOn items associated with this item.
        /// </summary>
        public List<PlayOnItem> Items
        {
            get
            {
                if (this._Items == null)
                {
                    _Items = new List<PlayOnItem>();
                    _API.GetItems(URL, _Items);
                }
                return _Items;
            }
        }

        /// <summary>
        /// Loads the child items from an XmlNode and stores them in the Items property.
        /// </summary>
        protected virtual void LoadItemsFromNode(XmlNode Node)
        {
            _Items = new List<PlayOnItem>();
            _API.GetItems(Node, _Items);
        }
    }

    /// <summary>
    /// Represents the root Catalog in PlayOn.  Access the Items property to see all the available channels.
    /// </summary>
    public class PlayOnCatalog : PlayOnFolder
    {
        private string _APIVersion;
        private string _ServerVersion;

        public PlayOnCatalog(PlayOn API)
            : base(API)
        {

        }

        public override void LoadFromNode(XmlNode Node)
        {
            _APIVersion = Util.GetNodeAttributeValue(Node, "apiVersion");
            _ServerVersion = Util.GetNodeAttributeValue(Node, "server");
        }

        /// <summary>
        /// The API version string of the connected PlayOn server.
        /// </summary>
        public string APIVersion { get { return _APIVersion; } }

        /// <summary>
        /// The version string of the connected PlayOn server.
        /// </summary>
        public string ServerVersion { get { return _ServerVersion; } }
    }

    /// <summary>
    /// Represents a playable video in PlayOn.
    /// </summary>
    public class PlayOnVideo : PlayOnItem
    {
        private bool LoadedDetails;
        private string _ArtURLLarge;
        private DateTime _AirDate;
        private string _Description;
        private string _MediaTitle;
        private string _MediaURL;
        private string _PlayLaterName;
        private string _PlayLaterURL;
        private string _RunTime;
        private string _Series;

        public PlayOnVideo(PlayOn API)
            : base(API)
        {
            LoadedDetails = false;
        }

        public override void LoadFromNode(XmlNode Node)
        {
            base.LoadFromNode(Node);

            if (Node.HasChildNodes) {
                _ArtURLLarge = Util.GetChildNodeAttributeValue(Node, "media", "art");
                _AirDate = DateTime.Parse(Util.GetChildNodeAttributeValue(Node, "date", "name"));
                _Description = Util.GetChildNodeAttributeValue(Node, "description","name");
                _MediaTitle = Util.GetChildNodeAttributeValue(Node, "media_title", "name");
                _MediaURL = Util.GetChildNodeAttributeValue(Node, "media", "src");
                _PlayLaterName = Util.GetChildNodeAttributeValue(Node, "media_playlater", "name");
                _PlayLaterURL = Util.GetChildNodeAttributeValue(Node, "media_playlater", "src");
                _RunTime = Util.GetChildNodeAttributeValue(Node, "time", "name");
                _Series = Util.GetChildNodeAttributeValue(Node, "series", "name");
            }
        }

        private void LoadDetails()
        {
            if (LoadedDetails)
                return;
            _API.LoadItemDetails(this);
            LoadedDetails = true;
        }

        /// <summary>
        /// A relative PlayOn URL for the large image for this video item.
        /// </summary>
        public string ArtURLLarge { get { LoadDetails(); return _ArtURLLarge; } }

        /// <summary>
        /// The date string representing the date this video item originally aired.
        /// </summary>
        public DateTime AirDate { get { LoadDetails(); return _AirDate; } }

        /// <summary>
        /// The full description for this video item.
        /// </summary>
        public string Description { get { LoadDetails(); return _Description; } }

        /// <summary>
        /// The full media title for this video item.
        /// </summary>
        public string MediaTitle { get { LoadDetails(); return _MediaTitle; } }

        /// <summary>
        /// A relative PlayOn URL for streaming the video of this item.
        /// </summary>
        public string MediaURL { get { LoadDetails(); return _MediaURL; } }

        /// <summary>
        /// The name of the PlayLater instance associated with the connected server.
        /// </summary>
        public string PlayLaterName { get { LoadDetails(); return _PlayLaterName; } }

        /// <summary>
        /// A relative PlayOn URL for queueing this video in PlayLater.
        /// </summary>
        public string PlayLaterURL { get { LoadDetails(); return _PlayLaterURL; } }

        /// <summary>
        /// The runtime of this video item.
        /// </summary>
        public string RunTime { get { LoadDetails(); return _RunTime; } }

        /// <summary>
        /// The series name associated with this video item.
        /// </summary>
        public string Series { get { LoadDetails(); return _Series; } }

        /// <summary>
        /// Attempts to queue this video item in PlayLater and indicates the result.
        /// </summary>
        public QueueVideoResult AddToPlayLaterQueue()
        {
            if (this.PlayLaterURL == "")
                return QueueVideoResult.PlayLaterNotFound;
            XmlDocument Doc = _API.XmlRequest(this.PlayLaterURL);
            bool Success = Util.GetNodeInnerText(Doc.SelectSingleNode("result/status")) == "true";
            string Message = Util.GetNodeInnerText(Doc.SelectSingleNode("result/msg"));
            if (Success)
                return QueueVideoResult.Success;
            else if (Message.IndexOf("already") > -1)
                return QueueVideoResult.AlreadyInQueue;
            else
                return QueueVideoResult.Failed;
        }
    }

    public enum QueueVideoResult { AlreadyInQueue, PlayLaterNotFound, Success, Failed };
   
}
