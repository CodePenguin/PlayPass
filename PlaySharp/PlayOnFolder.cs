using System.Collections.Generic;
using System.Xml;

namespace PlaySharp
{
    /// <summary>
    ///     Represents a folder in PlayOn.  The child items can be accessed by using the Items property.
    /// </summary>
    public class PlayOnFolder : PlayOnItem
    {
        private List<PlayOnItem> _items;

        public PlayOnFolder(PlayOn api)
            : base(api)
        {
        }

        /// <summary>
        ///     A list containing all child PlayOn items associated with this item.
        /// </summary>
        public IEnumerable<PlayOnItem> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new List<PlayOnItem>();
                    PlayOn.GetItems(Url, _items);
                }
                return _items;
            }
        }

        /// <summary>
        ///     Loads the child items from an XmlNode and stores them in the Items property.
        /// </summary>
        protected virtual void LoadItemsFromNode(XmlNode node)
        {
            _items = new List<PlayOnItem>();
            PlayOn.GetItems(node, _items);
        }
    }
}