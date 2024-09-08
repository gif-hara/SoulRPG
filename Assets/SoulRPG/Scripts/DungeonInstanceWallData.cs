using System.Collections.Generic;
using HK;
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

        public DungeonInstanceWallData(MasterData.WallEvent wallEvent)
        {
            From = new Vector2Int(wallEvent.LeftX, wallEvent.LeftY);
            To = new Vector2Int(wallEvent.RightX, wallEvent.RightY);
            EventType = wallEvent.EventType;
            PositiveSideCondition = wallEvent.PositiveSideCondition;
            NegativeSideCondition = wallEvent.NegativeSideCondition;
        }

        public DungeonInstanceWallData
        (
            Vector2Int from,
            Vector2Int to,
            string eventType,
            string positiveSideCondition,
            string negativeSideCondition,
            bool isOpen
        )
        {
            From = from;
            To = to;
            EventType = eventType;
            PositiveSideCondition = positiveSideCondition;
            NegativeSideCondition = negativeSideCondition;
            this.isOpen.Value = isOpen;
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
