using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace PlaySharp
{

    /// <summary>
    /// Constant values used throughout PlayOn.
    /// </summary>
    public class PlayOnConstants
    {
        public const string DefaultHost = "localhost";
        public const int DefaultPort = 54479;
        public const string DefaultURL = "/data/data.xml";
    }

    /// <summary>
    /// A class representing a connection to a PlayOn server.
    /// </summary>
    public class PlayOn
    {
        private System.Net.WebClient WebClient;

        /// <summary>
        /// The ip address or machine name that PlayOn Server is installed on.
        /// </summary>
        public string ServerHost;
        
        /// <summary>
        /// The port that PlayOn Server is listening to.
        /// </summary>
        public int ServerPort;

        /// <summary>
        /// Initializes a PlayOn object for the local machine and default port.
        /// </summary>
        public PlayOn()
        {
            ServerHost = PlayOnConstants.DefaultHost;
            ServerPort = PlayOnConstants.DefaultPort;

            WebClient = new System.Net.WebClient();
        }

        /// <summary>
        /// Initializes a PlayOn object for a specific server and port.
        /// </summary>
        public PlayOn(string Host, int Port)
            : this()
        {
            ServerHost = Host;
            ServerPort = Port;
        }

        ~PlayOn()
        {
            WebClient.Dispose();
        }

        /// <summary>
        /// Returns the root PlayOnCatalog item.
        /// </summary>
        PlayOnCatalog GetCatalog()
        {
            return (PlayOnCatalog)GetItem(PlayOnConstants.DefaultURL);
        }

        /// <summary>
        /// Returns a fully qualified URL from a relative PlayOn URL.
        /// </summary>
        /// <param name="URL">A relative PlayOn URL.</param>
        public string GetFullURL(string RelativeURL)
        {
            return String.Format("http://{0}:{1}/{2}", ServerHost, ServerPort, RelativeURL.TrimStart('/'));
        }

        /// <summary>
        /// Returns a PlayOnItem based on the data in an XmlNode.
        /// </summary>
        public PlayOnItem GetItem(XmlNode Node)
        {
            string NodeType = Util.GetNodeAttributeValue(Node, "type");
            PlayOnItem NewItem;
            if (Node.Name == "catalog")
                NewItem = new PlayOnCatalog(this);
            else if (NodeType == "folder")
                NewItem = new PlayOnFolder(this);
            else if (NodeType == "video")
                NewItem = new PlayOnVideo(this);
            else
                throw new Exception(String.Format("Unhandled node type: {0}", NodeType));
            NewItem.LoadFromNode(Node);
            return NewItem;
        }

        /// <summary>
        /// Returns a PlayOnItem based on the data from a URL.
        /// </summary>
        /// <param name="URL">A relative PlayOn URL.</param>
        public PlayOnItem GetItem(string URL)
        {
            XmlDocument Doc = XmlRequest(URL);
            return GetItem(Doc.ChildNodes[0]);
        }

        /// <summary>
        /// Creates PlayOnItems based on the data in the children of an XmlNode and adds them to a list.
        /// </summary>
        public void GetItems(XmlNode Node, List<PlayOnItem> List)
        {
            foreach (XmlNode ChildNode in Node.SelectNodes("group"))
                List.Add(GetItem(ChildNode));
        }

        /// <summary>
        /// Creates PlayOnItems based on the data at a URL and adds them to a list.
        /// </summary>
        /// <param name="URL">A relative PlayOn URL.</param>
        public void GetItems(string URL, List<PlayOnItem> List)
        {
            XmlDocument Doc = XmlRequest(URL);
            GetItems(Doc.ChildNodes[0], List);
        }

        /// <summary>
        /// Loads the full details of a PlayOnItem.
        /// </summary>
        public void LoadItemDetails(PlayOnItem Item)
        {
            XmlDocument Doc = XmlRequest(Item.URL);
            Item.LoadFromNode(Doc.ChildNodes[0]);
        }

        /// <summary>
        /// Returns an XmlDocument from a URL.
        /// </summary>
        /// <param name="URL">A relative PlayOn URL.</param>
        public XmlDocument XmlRequest(string URL)
        {
            XmlDocument XmlDoc = new XmlDocument();
            MemoryStream stream = new MemoryStream(WebClient.DownloadData(GetFullURL(URL)));
            XmlDoc.Load(stream);
            return XmlDoc;
        }
    }

}
