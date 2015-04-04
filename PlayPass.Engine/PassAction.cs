using System.Collections.Generic;

namespace PlayPass.Engine
{
    /// <summary>
    ///     A basic data structure that represents an action to perform.  PassActions can be nested inside of each other.
    /// </summary>
    public class PassAction
    {
        public PassAction(string name, PassActionType type)
        {
            Actions = new PassActions();
            Name = name;
            Type = type;
        }

        public string Name { get; private set; }
        public PassActionType Type { get; private set; }
        public PassActions Actions { get; private set; }
    }

    public class PassActions : List<PassAction>
    {
    }

    public enum PassActionType
    {
        Queue,
        Scan
    };
}