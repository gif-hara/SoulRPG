using System.Collections.Generic;
using R3;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class DungeonInstanceWallData
    {
        public Vector2Int From { get; }
        
        public Vector2Int To { get; }
        
        public string EventType { get; }
        
        public string PositiveSideCondition { get; }
        
        public string NegativeSideCondition { get; }
        
        private readonly ReactiveProperty<bool> isOpen = new();
        public ReadOnlyReactiveProperty<bool> IsOpenReactiveProperty => isOpen.ToReadOnlyReactiveProperty();
        public bool IsOpen => isOpen.Value;
        
        public List<NeedItem> NeedItems { get; } = new();

        public DungeonInstanceWallData(MasterData.WallEvent wallEventData)
        {
            From = new Vector2Int(wallEventData.LeftX, wallEventData.LeftY);
            To = new Vector2Int(wallEventData.RightX, wallEventData.RightY);
            EventType = wallEventData.EventType;
            PositiveSideCondition = wallEventData.PositiveSideCondition;
            NegativeSideCondition = wallEventData.NegativeSideCondition;
        }

        public bool IsPositiveAccess(Define.Direction accessDirection)
        {
            var isHorizontal = From.y == To.y;
            return isHorizontal ? accessDirection == Define.Direction.Down : accessDirection == Define.Direction.Right;
        }

        public void Open()
        {
            isOpen.Value = true;
        }
    }
}
