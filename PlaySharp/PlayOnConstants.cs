namespace PlaySharp
{
    /// <summary>
    ///     Constant values used throughout PlayOn.
    /// </summary>
    public static class PlayOnConstants
    {
        public enum QueueVideoResult
        {
            AlreadyInQueue,
            PlayLaterNotFound,
            Success,
            Failed
        };

        public const string DefaultHost = "localhost";
        public const int DefaultPort = 54479;
        public const string DefaultUrl = "/data/data.xml";
    }
}