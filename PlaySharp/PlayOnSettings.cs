using Microsoft.Win32;

namespace PlaySharp
{

    /// <summary>
    /// A static class used to get settings from the locally installed PlayOn instance.
    /// </summary>
    public static class PlayOnSettings
    {
        /// <summary>
        /// The registry key where the PlayOn settings reside.
        /// </summary>
        public const string PlayOnRegistryKey = "Software\\MediaMall\\MediaMall\\CurrentVersion\\Settings";

        /// <summary>
        /// Gets the PlayOn Media Storage Location value from the registry.
        /// </summary>
        public static string GetMediaStorageLocation()
        {
            RegistryKey Reg = Registry.LocalMachine.OpenSubKey(PlayOnRegistryKey);
            foreach (string Item in Reg.GetValue("mediaStoragePaths").ToString().Split('*'))
                if (System.IO.Directory.Exists(Item))
                    return Item;
            return "";
        }

        /// <summary>
        /// Gets the PlayOn Video Format value from the registry.
        /// </summary>
        public static string GetPlayLaterVideoFormat()
        {
            RegistryKey Reg = Registry.LocalMachine.OpenSubKey(PlayOnRegistryKey);
            if (Reg.GetValue("playLaterVideoFormat", 0).ToString() == "1")
                return ".plv";
            else
                return ".mp4";
        }
    }

}
