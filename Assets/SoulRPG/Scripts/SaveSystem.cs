using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SaveSystem
    {
        public static void Save<T>(T data, string path)
        {
            var json = JsonUtility.ToJson(data);
            var encryptedJson = EncryptionUtility.Encrypt(json);
            PlayerPrefs.SetString(path, encryptedJson);
        }
        
        public static T Load<T>(string path)
        {
            if (!Contains(path))
            {
                return default;
            }
            var encryptedJson = PlayerPrefs.GetString(path);
            var json = EncryptionUtility.Decrypt(encryptedJson);
            return JsonUtility.FromJson<T>(json);
        }
        
        public static bool Contains(string path)
        {
            return PlayerPrefs.HasKey(path);
        }
        
        public static void Delete(string path)
        {
            PlayerPrefs.DeleteKey(path);
        }
    }
}
