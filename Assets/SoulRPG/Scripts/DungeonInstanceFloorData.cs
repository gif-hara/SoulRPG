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

        public DungeonInstanceFloorData(Vector2Int position, List<(MasterData.Item item, int count)> items)
        {
            Position = position;
            EventType = "Item";
            Items = items;
        }

        public DungeonInstanceFloorData(Vector2Int position, int enemyId)
        {
            Position = position;
            EventType = "Enemy";
            EnemyId = enemyId;
        }
    }
}
