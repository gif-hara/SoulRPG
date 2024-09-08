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

        public SuspendData suspendData;

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
        }

        [Serializable]
        public class SuspendData
        {
            public CharacterGrowthParameter growthParameter;

            public Vector2Int playerPosition;

            public Define.Direction playerDirection;

            public EquipmentData playerEquipmentData;

            public ItemData[] playerItemData;

            public PlayerInstanceData playerInstanceData;

            public DungeonData dungeonData;
        }

        [Serializable]
        public class EquipmentData
        {
            public int[] weaponIds;

            public int headId;

            public int bodyId;

            public int armId;

            public int legId;

            public int[] accessoryIds;
        }

        [Serializable]
        public class ItemData
        {
            public int itemId;

            public int count;
        }

        [Serializable]
        public class PlayerInstanceData
        {
            public int hitPoint;

            public int stamina;

            public int experience;
        }

        [Serializable]
        public class DungeonData
        {
            public string dungeonName;

            public int floorId;

            public DungeonInstanceItemData[] itemData;

            public DungeonInstanceSequenceEventData[] sequenceEventData;
        }

        [Serializable]
        public class DungeonInstanceItemData
        {
            public Vector2Int position;

            public int itemId;

            public int count;
        }

        [Serializable]
        public class DungeonInstanceSequenceEventData
        {
            public Vector2Int position;

            public string viewName;

            public string promptMessage;

            public string sequenceId;
        }
    }
}
