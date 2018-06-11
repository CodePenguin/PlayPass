using System;

namespace PlayPass.Engine
{
    public class PassQueueAction : PassAction
    {
        public int CountLimit { get; set; }
        public TimeSpan DurationLimit { get; set; }

        public PassQueueAction()
        {
            Type = PassActionType.Queue;
        }
    }
}
