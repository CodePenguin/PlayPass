using System.Collections.Generic;

namespace PlayPass.Engine
{
    /// <summary>
    ///     A basic data structure that represents an action to perform.  PassActions can be nested inside of each other.
    /// </summary>
    public abstract class PassAction
    {
        protected PassAction()
        {
            Name = "";
            Exclude = "";
            Actions = new PassActions();
        }

        public string Name { get; set; }
        public string Exclude { get; set; }
        public PassActionType Type { get; protected set; }
        public PassActions Actions { get; }
    }

    public class PassActions : List<PassAction>
    {
    }

    public enum PassActionType
    {
        Queue,
        Scan,
        Search
    };
}