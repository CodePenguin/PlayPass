using System.Collections.Generic;

namespace PlayPass.Engine
{
    /// <summary>
    ///     A basic data structure that represents an action to perform.  PassActions can be nested inside of each other.
    /// </summary>
    public class PassAction
    {
        public PassAction()
        {
            Actions = new PassActions();
        }

        public string Name { get; set; }
        public string Exclude { get; set; }
        public PassActionType Type { get; set; }
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