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

        public DungeonInstanceFloorData(Vector2Int position, string eventType, List<(MasterData.Item item, int count)> items)
        {
            Position = position;
            EventType = eventType;
            Items = items;
        }
    }
}
