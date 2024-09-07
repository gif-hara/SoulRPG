#define ENABLE_SAVE
using System;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class SaveData
    {
        public PlayerData playerData;

        public static void Save(SaveData saveData)
        {
#if ENABLE_SAVE
            var json = JsonUtility.ToJson(saveData);
            var encryptedJson = EncryptionUtility.Encrypt(json);
            PlayerPrefs.SetString("SaveData", encryptedJson);
#endif
        }

        public static SaveData Load()
        {
            if (!Contains())
            {
                return null;
            }
            var encryptedJson = PlayerPrefs.GetString("SaveData");
            var json = EncryptionUtility.Decrypt(encryptedJson);
            return JsonUtility.FromJson<SaveData>(json);
        }

        public static bool Contains()
        {
            return PlayerPrefs.HasKey("SaveData");
        }

        [Serializable]
        public class PlayerData
        {
            public string name;

            public CharacterGrowthParameter growthParameter;
        }
    }
}
