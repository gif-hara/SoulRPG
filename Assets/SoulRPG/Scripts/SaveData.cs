#define ENABLE_SAVE
using System;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class SaveData : ISaveData
    {
        public PlayerData playerData;
        
        public static SaveData Load()
        {
            return SaveSystem.Load<SaveData>("SaveData");
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
        }

        public string DefaultPath => "SaveData";
    }
}
