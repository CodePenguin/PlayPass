using System.IO;
using Microsoft.Win32;

namespace PlaySharp
{
    /// <summary>
    ///     A static class used to get settings from the locally installed PlayOn instance.
    /// </summary>
    public static class PlayOnSettings
    {
        /// <summary>
        ///     Gets the PlayOn Media Storage Location value from the registry.
        /// </summary>
        public static string GetMediaStorageLocation()
        {
            var reg = Registry.LocalMachine.OpenSubKey(GetPlayOnSettingsRegistryKey());
            if (reg == null)
                return "";
            var regValue = reg.GetValue("myRecordingsPath");
            if (regValue == null)
                return "";
            foreach (var item in regValue.ToString().Split('*'))
                if (Directory.Exists(item))
                    return item;
            return "";
        }

        /// <summary>
        ///     Gets the PlayOn settings registry key.
        /// </summary>
        public static string GetPlayOnSettingsRegistryKey()
        {
            const string mediaMallBaseKey = "Software\\{0}MediaMall\\MediaMall\\CurrentVersion\\Settings";
            var key = string.Format(mediaMallBaseKey, "Wow6432Node\\");
            var reg = Registry.LocalMachine.OpenSubKey(key);
            if (reg == null)
                key = string.Format(mediaMallBaseKey, "");
            return key;
        }

        /// <summary>
        ///     Gets the PlayOn Video Format value from the registry.
        /// </summary>
        public static string GetPlayLaterVideoFormat()
        {
            var reg = Registry.LocalMachine.OpenSubKey(GetPlayOnSettingsRegistryKey());
            if (reg != null)
                return reg.GetValue("playLaterVideoFormat", 0).ToString() == "1" ? ".plv" : ".mp4";
            return "";
        }
    }
}