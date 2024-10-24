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

        public GameSettingData gameSettingData;

        public LanguageData languageData;

        public static SaveData LoadSafe()
        {
            var result = SaveSystem.Load<SaveData>("SaveData");
            if (result == null)
            {
                result = new SaveData
                {
                    playerData = new PlayerData(),
                    audioData = new AudioData(),
                    gameSettingData = new GameSettingData(),
                    languageData = new LanguageData()
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

            public int badEndCount;

            public bool isTrueEndRoute;
        }

        [Serializable]
        public class AudioData
        {
            public float masterVolume = 0.5f;

            public float bgmVolume = 0.3f;

            public float sfxVolume = 0.8f;
        }

        [Serializable]
        public class GameSettingData
        {
            public bool isRotationMiniMap = true;
        }

        [Serializable]
        public class LanguageData
        {
            public string language;
        }

        public string DefaultPath => "SaveData";
    }
}
