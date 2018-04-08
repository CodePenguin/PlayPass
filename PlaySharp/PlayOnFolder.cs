using System.Collections.Generic;
using System.Xml;

namespace PlaySharp
{
    /// <summary>
    ///     Represents a folder in PlayOn.  The child items can be accessed by using the Items property.
    /// </summary>
    public class PlayOnFolder : PlayOnItem
    {
        private IList<PlayOnItem> _items;

        public PlayOnFolder(IPlayOn api) : base(api)
        {
        }

        public PlayOnFolder(IPlayOn api, IList<PlayOnItem> items) : base(api)
        {
            _items = items;
        }

        /// <summary>
        ///     A list containing all child PlayOn items associated with this item.
        /// </summary>
        public IList<PlayOnItem> Items
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
        ///     Indicates if the folder is searchable.
        /// </summary>
        public bool Searchable { get; set; }

        public override void LoadFromNode(XmlNode node)
        {
            base.LoadFromNode(node);
            Searchable = Util.GetNodeAttributeValue(node, "searchable") == "true";
        }

        /// <summary>
        ///     Returns a PlayOnSearch object based on the current folder and search term.
        /// </summary>
        public PlayOnFolder Search(string searchTerm)
        {
            return PlayOn.GetSearchResults(this.Url, searchTerm);
        }
    }
}