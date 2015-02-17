using Microsoft.Win32;

namespace PlaySharp
{

    /// <summary>
    /// A static class used to get settings from the locally installed PlayOn instance.
    /// </summary>
    public static class PlayOnSettings
    {
        /// <summary>
        /// Gets the PlayOn Media Storage Location value from the registry.
        /// </summary>
        public static string GetMediaStorageLocation()
        {
            RegistryKey Reg = Registry.LocalMachine.OpenSubKey(GetPlayOnSettingsRegistryKey());
            foreach (string Item in Reg.GetValue("mediaStoragePaths").ToString().Split('*'))
                if (System.IO.Directory.Exists(Item))
                    return Item;
            return "";
        }

        /// <summary>
        /// Gets the PlayOn settings registry key.
        /// </summary>
        public static string GetPlayOnSettingsRegistryKey()
        {
            const string MediaMallBaseKey = "Software\\{0}MediaMall\\MediaMall\\CurrentVersion\\Settings";
            string Key = string.Format(MediaMallBaseKey, "Wow6432Node\\");
            RegistryKey Reg = Registry.LocalMachine.OpenSubKey(Key);
            if (Reg == null)
                Key =  string.Format(MediaMallBaseKey, "");
            return Key;
        }

        /// <summary>
        /// Gets the PlayOn Video Format value from the registry.
        /// </summary>
        public static string GetPlayLaterVideoFormat()
        {
            RegistryKey Reg = Registry.LocalMachine.OpenSubKey(GetPlayOnSettingsRegistryKey());
            if (Reg.GetValue("playLaterVideoFormat", 0).ToString() == "1")
                return ".plv";
            else
                return ".mp4";
        }

    }

}
