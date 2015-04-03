using System.Collections.Generic;

namespace PlayPass
{
    class PassAction
    {
        public string Name { get; set; }
        public PassActionType Type { get; set; }
        public PassActions Actions { get; set; }

        public PassAction(string name, PassActionType type)
        {
            Actions = new PassActions();
            Name = name;
            Type = type;
        }
    }

    class PassActions : List<PassAction>
    {
        
    }

    public enum PassActionType
    {
        Queue,
        Scan
    };
}
