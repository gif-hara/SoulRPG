using UnityEngine;
using UnityEngine.Localization.Settings;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        public static string Localized(this string self)
        {
#if UNITY_WEBGL
            return self;
#else
            var table = LocalizationSettings.StringDatabase.GetTable("TextData");
            var entry = table.GetEntry(self);
            if (entry == null)
            {
                Debug.LogWarning($"Not found key: {self}");
                return self;
            }
            return entry.Value;
#endif
        }

        public static string Format(this string self, params object[] args)
        {
            return string.Format(self, args);
        }
    }
}