using System.Collections.Generic;

namespace PlayPassEngine
{
    public class PassItem
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

    public class PassItems : List<PassItem>
    {
        
    }
}
