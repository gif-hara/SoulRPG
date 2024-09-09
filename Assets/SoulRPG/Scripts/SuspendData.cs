using System;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class SuspendData
    {
        public CharacterGrowthParameter growthParameter;

        public Vector2Int playerPosition;

        public Define.Direction playerDirection;

        public EquipmentData playerEquipmentData;

        public ItemData[] playerItemData;

        public PlayerInstanceData playerInstanceData;

        public DungeonData dungeonData;

        public static void Save(SuspendData data)
        {
            var json = JsonUtility.ToJson(data);
            var encryptedJson = EncryptionUtility.Encrypt(json);
            PlayerPrefs.SetString("SuspendData", encryptedJson);
        }
        
        public static SuspendData Load()
        {
            if (!Contains())
            {
                return null;
            }
            var encryptedJson = PlayerPrefs.GetString("SuspendData");
            var json = EncryptionUtility.Decrypt(encryptedJson);
            return JsonUtility.FromJson<SuspendData>(json);
        }
        
        public static bool Contains()
        {
            return PlayerPrefs.HasKey("SuspendData");
        }
        
        public static void Delete()
        {
            PlayerPrefs.DeleteKey("SuspendData");
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

            public DungeonEnemyData[] enemyData;

            public DungeonInstanceWallData[] wallData;

            public ReachedPositionData[] reachedPositionData;
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

        [Serializable]
        public class DungeonEnemyData
        {
            public Vector2Int position;

            public int enemyId;

            public bool findPlayer;
        }

        [Serializable]
        public class DungeonInstanceWallData
        {
            public Vector2Int from;

            public Vector2Int to;

            public string eventType;

            public string positiveSideCondition;

            public string negativeSideCondition;

            public bool isOpen;
        }

        [Serializable]
        public class ReachedPositionData
        {
            public Vector2Int position;
        }
    }
}
