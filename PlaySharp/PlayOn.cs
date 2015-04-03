using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;

namespace PlaySharp
{
    /// <summary>
    ///     A class representing a connection to a PlayOn server.
    /// </summary>
    public class PlayOn
    {
        private readonly WebClient _webClient;

        /// <summary>
        ///     The ip address or machine name that PlayOn Server is installed on.
        /// </summary>
        public string ServerHost { get; set; }

        /// <summary>
        ///     The port that PlayOn Server is listening to.
        /// </summary>
        public int ServerPort { get; set; }

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
        public PlayOn(string host, int port)
            : this()
        {
            ServerHost = host;
            ServerPort = port;
        }

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
            return (PlayOnCatalog) GetItem(PlayOnConstants.DefaultUrl);
        }

        /// <summary>
        ///     Returns a fully qualified URL from a relative PlayOn URL.
        /// </summary>
        /// <param name="relativeUrl">A relative PlayOn URL.</param>
        public string GetFullUrl(string relativeUrl)
        {
            return String.Format("http://{0}:{1}/{2}", ServerHost, ServerPort, relativeUrl.TrimStart('/'));
        }

        /// <summary>
        ///     Returns a PlayOnItem based on the data in an XmlNode.
        /// </summary>
        public PlayOnItem GetItem(XmlNode node)
        {
            var nodeType = Util.GetNodeAttributeValue(node, "type");
            PlayOnItem newItem;
            if (node.Name == "catalog")
                newItem = new PlayOnCatalog(this);
            else if (nodeType == "folder")
                newItem = new PlayOnFolder(this);
            else if (nodeType == "video")
                newItem = new PlayOnVideo(this);
            else
                throw new Exception(String.Format("Unhandled node type: {0}", nodeType));
            newItem.LoadFromNode(node);
            return newItem;
        }

        /// <summary>
        ///     Returns a PlayOnItem based on the data from a URL.
        /// </summary>
        /// <param name="url">A relative PlayOn URL.</param>
        public PlayOnItem GetItem(string url)
        {
            var doc = XmlRequest(url);
            return GetItem(doc.ChildNodes[0]);
        }

        /// <summary>
        ///     Creates PlayOnItems based on the data in the children of an XmlNode and adds them to a list.
        /// </summary>
        public void GetItems(XmlNode node, List<PlayOnItem> list)
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
        public void GetItems(string url, List<PlayOnItem> list)
        {
            var doc = XmlRequest(url);
            GetItems(doc.ChildNodes[0], list);
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
        public XmlDocument XmlRequest(string url)
        {
            var xmlDoc = new XmlDocument();
            var stream = new MemoryStream(_webClient.DownloadData(GetFullUrl(url)));
            xmlDoc.Load(stream);
            return xmlDoc;
        }
    }
}