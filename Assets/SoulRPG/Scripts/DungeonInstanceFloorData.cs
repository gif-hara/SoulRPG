using System.Collections.Generic;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class DungeonInstanceFloorData
    {
        public Vector2Int Position { get; }

        public string EventType { get; }

        public List<(MasterData.Item item, int count)> Items { get; } = new();

        public int EnemyId { get; set; }

        public static DungeonInstanceFloorData CreateAsItem(Vector2Int position, List<(MasterData.Item item, int count)> items)
        {
            return new DungeonInstanceFloorData(position, items);
        }

        public static DungeonInstanceFloorData CreateAsEnemy(Vector2Int position, int enemyId)
        {
            return new DungeonInstanceFloorData(position, enemyId);
        }

        public static DungeonInstanceFloorData CreateAsSavePoint(Vector2Int position)
        {
            return new DungeonInstanceFloorData(position, "SavePoint");
        }

        private DungeonInstanceFloorData(Vector2Int position, List<(MasterData.Item item, int count)> items)
        {
            Position = position;
            EventType = "Item";
            Items = items;
        }

        private DungeonInstanceFloorData(Vector2Int position, int enemyId)
        {
            Position = position;
            EventType = "Enemy";
            EnemyId = enemyId;
        }

        private DungeonInstanceFloorData(Vector2Int position, string eventType)
        {
            Position = position;
            EventType = eventType;
        }
    }
}
