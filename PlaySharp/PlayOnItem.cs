using System.Xml;

namespace PlaySharp
{
    /// <summary>
    ///     The base class that all PlayOn items inherit from.
    /// </summary>
    public class PlayOnItem
    {
        protected readonly IPlayOn PlayOn;

        protected PlayOnItem(IPlayOn playOn)
        {
            PlayOn = playOn;
        }

        /// <summary>
        ///     A relative PlayOn URL for a small image of the item.
        /// </summary>
        public string ArtUrl { get; set; }

        /// <summary>
        ///     The display name of the PlayOn item.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     A relative PlayOn URL for the details of this item.
        /// </summary>
        public string Url { get; set; }

        public virtual void LoadFromNode(XmlNode node)
        {
            Name = Util.GetNodeAttributeValue(node, "name");
            Url = Util.GetNodeAttributeValue(node, "href");
            ArtUrl = Util.GetNodeAttributeValue(node, "art");
        }
    }
}