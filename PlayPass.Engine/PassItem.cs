using System.Collections.Generic;

namespace PlayPass.Engine
{
    /// <summary>
    ///     A basic data structure to represent passes.  Passes can hold many PassActions.
    /// </summary>
    public class PassItem
    {
        public PassItem(string description, bool enabled)
        {
            Actions = new PassActions();
            Description = description;
            Enabled = enabled;
        }

        public PassActions Actions { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
    }

    public class PassItems : List<PassItem>
    {
    }
}