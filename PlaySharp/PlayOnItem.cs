using System.Xml;

namespace PlaySharp
{
    /// <summary>
    ///     The base class that all PlayOn items inherit from.
    /// </summary>
    public class PlayOnItem
    {
        protected readonly PlayOn PlayOn;

        protected PlayOnItem(PlayOn playOn)
        {
            PlayOn = playOn;
        }

        /// <summary>
        ///     A relative PlayOn URL for a small image of the item.
        /// </summary>
        public string ArtUrl { get; private set; }

        /// <summary>
        ///     The display name of the PlayOn item.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     A relative PlayOn URL for the details of this item.
        /// </summary>
        public string Url { get; private set; }

        public virtual void LoadFromNode(XmlNode node)
        {
            Name = Util.GetNodeAttributeValue(node, "name");
            Url = Util.GetNodeAttributeValue(node, "href");
            ArtUrl = Util.GetNodeAttributeValue(node, "art");
        }
    }
}