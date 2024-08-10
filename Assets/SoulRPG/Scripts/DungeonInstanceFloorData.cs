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

        public DungeonInstanceFloorData(MasterData.FloorEvent floorEvent)
        {
            Position = new Vector2Int(floorEvent.X, floorEvent.Y);
            EventType = floorEvent.EventType;
        }

        public DungeonInstanceFloorData(MasterData.FloorItem floorItem)
        {
            Position = new Vector2Int(floorItem.X, floorItem.Y);
            EventType = "Item";
        }
    }
}
