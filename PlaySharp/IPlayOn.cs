using System.Collections.Generic;

namespace PlaySharp
{
    public interface IPlayOn
    {
        PlayOnCatalog GetCatalog();
        void GetItems(string url, IList<PlayOnItem> list);
        PlayOnFolder GetSearchResults(string url, string searchTerm);
        void LoadItemDetails(PlayOnItem item);
        PlayOnConstants.QueueVideoResult QueueMedia(PlayOnVideo media);
    }
}
