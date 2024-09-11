#define ENABLE_SAVE
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class SaveData : ISaveData
    {
        public PlayerData playerData;

        public AudioData audioData;

        public static SaveData LoadSafe()
        {
            var result = SaveSystem.Load<SaveData>("SaveData");
            if (result == null)
            {
                result = new SaveData
                {
                    playerData = new PlayerData(),
                    audioData = new AudioData()
                };
                result.Save();
            }
            return result;
        }

        public static bool Contains()
        {
            return SaveSystem.Contains("SaveData");
        }

        public static void Delete()
        {
            SaveSystem.Delete("SaveData");
        }

        [Serializable]
        public class PlayerData
        {
            public string name;

            public string sealedName;
        }

        [Serializable]
        public class AudioData
        {
            public float masterVolume = 0.8f;

            public float bgmVolume = 0.8f;

            public float sfxVolume = 0.8f;
        }

        public string DefaultPath => "SaveData";
    }
}
