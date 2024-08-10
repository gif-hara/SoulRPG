using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class DungeonInstanceFloorData
    {
        public Vector2Int Position { get; }

        public string EventTypeName { get; }

        public DungeonInstanceFloorData(MasterData.FloorEvent floorEvent)
        {
            Position = new Vector2Int(floorEvent.X, floorEvent.Y);
            EventTypeName = floorEvent.EventType;
        }
    }
}
