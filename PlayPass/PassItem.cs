using System.Collections.Generic;

namespace PlayPass
{
    class PassItem
    {
        public PassActions Actions { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }

        public PassItem(string description, bool enabled)
        {
            Actions = new PassActions();
            Description = description;
            Enabled = enabled;
        }
    }

    class PassItems : List<PassItem>
    {
        
    }
}
