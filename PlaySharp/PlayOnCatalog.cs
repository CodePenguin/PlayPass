using System.Collections.Generic;
using System.Xml;

namespace PlaySharp
{
    /// <summary>
    ///     Represents the root Catalog in PlayOn.  Access the Items property to see all the available channels.
    /// </summary>
    public class PlayOnCatalog : PlayOnFolder
    {
        public PlayOnCatalog(PlayOn api, IList<PlayOnItem> items = null) : base(api, items)
        {
        }

        /// <summary>
        ///     The API version string of the connected PlayOn server.
        /// </summary>
        public string ApiVersion { get; private set; }

        /// <summary>
        ///     The version string of the connected PlayOn server.
        /// </summary>
        public string ServerVersion { get; private set; }

        public override void LoadFromNode(XmlNode node)
        {
            base.LoadFromNode(node);
            ApiVersion = Util.GetNodeAttributeValue(node, "apiVersion");
            ServerVersion = Util.GetNodeAttributeValue(node, "server");
        }
    }
}