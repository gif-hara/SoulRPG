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

        public DungeonInstanceFloorData(MasterData.FloorItem floorItem)
        {
            Position = new Vector2Int(floorItem.X, floorItem.Y);
            EventType = "Item";
        }
    }
}
