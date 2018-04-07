using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;

namespace PlaySharp
{
    /// <summary>
    ///     A class representing a connection to a PlayOn server.
    /// </summary>
    public class PlayOn : IPlayOn
    {
        private readonly WebClient _webClient;

        public event XmlRequestEventHandler XmlRequestEvent;

        /// <summary>
        ///     Initializes a PlayOn object for the local machine and default port.
        /// </summary>
        public PlayOn()
        {
            ServerHost = PlayOnConstants.DefaultHost;
            ServerPort = PlayOnConstants.DefaultPort;

            _webClient = new WebClient();
        }

        /// <summary>
        ///     Initializes a PlayOn object for a specific server and port.
        /// </summary>
        public PlayOn(string host, int port) : this()
        {
            ServerHost = host;
            ServerPort = port;
        }

        /// <summary>
        ///     The ip address or machine name that PlayOn Server is installed on.
        /// </summary>
        public string ServerHost { get; set; }

        /// <summary>
        ///     The port that PlayOn Server is listening to.
        /// </summary>
        public int ServerPort { get; set; }

        /// <summary>
        ///     Deinitializes the PlayOn object.
        /// </summary>
        ~PlayOn()
        {
            _webClient.Dispose();
        }

        /// <summary>
        ///     Returns the root PlayOnCatalog item.
        /// </summary>
        public PlayOnCatalog GetCatalog()
        {
            return (PlayOnCatalog)GetItem(PlayOnConstants.DefaultUrl);
        }

        /// <summary>
        ///     Returns a fully qualified URL from a relative PlayOn URL.
        /// </summary>
        /// <param name="relativeUrl">A relative PlayOn URL.</param>
        private string GetFullUrl(string relativeUrl)
        {
            return String.Format("http://{0}:{1}/{2}", ServerHost, ServerPort, relativeUrl.TrimStart('/'));
        }

        /// <summary>
        ///     Returns a PlayOnItem based on the data in an XmlNode.
        /// </summary>
        private PlayOnItem GetItem(XmlNode node)
        {
            var nodeType = Util.GetNodeAttributeValue(node, "type");
            PlayOnItem newItem;
            switch (nodeType)
            {
                case "folder":
                    IList<PlayOnItem> childItems = null;
                    if (node.HasChildNodes)
                    {
                        childItems = new List<PlayOnItem>();
                        GetItems(node, childItems);
                    }

                    newItem = (node.Name == "catalog")
                        ? new PlayOnCatalog(this, childItems)
                        : new PlayOnFolder(this, childItems);
                    break;
                case "video":
                    newItem = new PlayOnVideo(this);
                    break;
                default:
                    throw new Exception(String.Format("Unhandled node type: {0}", nodeType));
            }
            newItem.LoadFromNode(node);
            return newItem;
        }

        /// <summary>
        ///     Returns a PlayOnItem based on the data from a URL.
        /// </summary>
        /// <param name="url">A relative PlayOn URL.</param>
        private PlayOnItem GetItem(string url)
        {
            var doc = XmlRequest(url);
            return GetItem(doc.ChildNodes[0]);
        }

        /// <summary>
        ///     Creates PlayOnItems based on the data in the children of an XmlNode and adds them to a list.
        /// </summary>
        private void GetItems(XmlNode node, IList<PlayOnItem> list)
        {
            var groupNodes = node.SelectNodes("group");
            if (groupNodes == null)
                return;
            foreach (XmlNode childNode in groupNodes)
                list.Add(GetItem(childNode));
        }

        /// <summary>
        ///     Creates PlayOnItems based on the data at a URL and adds them to a list.
        /// </summary>
        /// <param name="url">A relative PlayOn URL.</param>
        /// <param name="list">The list that PlayOnItems will be added to</param>
        public void GetItems(string url, IList<PlayOnItem> list)
        {
            var doc = XmlRequest(url);
            GetItems(doc.ChildNodes[0], list);
        }

        /// <summary>
        ///     Creates a PlayOnFolder representing the search results for the specified folder and search term.
        /// </summary>
        /// <param name="url">A relative PlayOn URL.</param>
        /// <param name="searchTerm">The text that will be searched for.</param>
        public PlayOnFolder GetSearchResults(string url, string searchTerm)
        {
            var searchCriteria = HttpUtility.UrlEncode("dc:description contains " + searchTerm);
            var searchUrl = String.Format("{0}&searchterm={1})", url, searchCriteria);
            return GetItem(searchUrl) as PlayOnFolder;
        }

        /// <summary>
        ///     Loads the full details of a PlayOnItem.
        /// </summary>
        public void LoadItemDetails(PlayOnItem item)
        {
            var doc = XmlRequest(item.Url);
            item.LoadFromNode(doc.ChildNodes[0]);
        }

        /// <summary>
        ///     Attempts to queue this video item in PlayLater and indicates the result.
        /// </summary>
        public PlayOnConstants.QueueVideoResult QueueMedia(PlayOnVideo media)
        {
            if (media.PlayLaterUrl == "")
                return PlayOnConstants.QueueVideoResult.PlayLaterNotFound;
            var doc = XmlRequest(media.PlayLaterUrl);
            var success = (Util.GetNodeInnerText(doc.SelectSingleNode("result/status")) == "true");
            var message = Util.GetNodeInnerText(doc.SelectSingleNode("result/msg"));
            if (success)
                return PlayOnConstants.QueueVideoResult.Success;
            if (message.IndexOf("already", StringComparison.Ordinal) > -1)
                return PlayOnConstants.QueueVideoResult.AlreadyInQueue;
            return PlayOnConstants.QueueVideoResult.Failed;
        }

        /// <summary>
        ///     Returns an XmlDocument from a URL.
        /// </summary>
        /// <param name="url">A relative PlayOn URL.</param>
        private XmlDocument XmlRequest(string url)
        {
            var xmlDoc = new XmlDocument();
            var stream = new MemoryStream(_webClient.DownloadData(GetFullUrl(url)));
            xmlDoc.Load(stream);

            if (XmlRequestEvent != null)
                XmlRequestEvent(this, new XmlRequestEventArgs() { RequestUrl = url, Xml = xmlDoc });

            return xmlDoc;
        }
    }
}