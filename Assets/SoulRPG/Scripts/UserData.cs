using System.Collections.Generic;
using HK;
using R3;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UserData
    {
        private readonly HashSet<string> completedFloorEventIds = new();

        /// <summary>
        /// 一時的に完了した床イベントのリスト
        /// </summary>
        /// <remarks>
        /// このイベントはセーブポイントにアクセスすると開放されて再度アクセス出来るようになります
        /// </remarks>
        private readonly HashSet<string> temporaryCompletedFloorEventIds = new();

        private readonly HashSet<string> completedWallEventIds = new();

        private readonly Dictionary<string, HashSet<Vector2Int>> reachedPoints = new();

        private readonly ReactiveProperty<int> experience = new(0);
        public ReadOnlyReactiveProperty<int> Experience => experience;

        public void AddCompletedfloorEventIds(string eventId, bool isOneTime)
        {
            if (isOneTime)
            {
                completedFloorEventIds.Add(eventId);
            }
            else
            {
                temporaryCompletedFloorEventIds.Add(eventId);
            }
        }

        public bool ContainsCompletedFloorEventId(string eventId)
        {
            return completedFloorEventIds.Contains(eventId) || temporaryCompletedFloorEventIds.Contains(eventId);
        }

        public void ClearTemporaryCompletedFloorEventIds()
        {
            var tempData = new HashSet<string>(temporaryCompletedFloorEventIds);
            temporaryCompletedFloorEventIds.Clear();
            TinyServiceLocator.Resolve<GameEvents>().OnClearTemporaryCompletedEventIds.OnNext(tempData);
        }

        public void AddCompletedWallEventIds(string eventId)
        {
            completedWallEventIds.Add(eventId);
        }

        public bool ContainsCompletedWallEventId(string eventId)
        {
            return completedWallEventIds.Contains(eventId);
        }

        public void AddReachedPoint(string dungeonName, Vector2Int point)
        {
            if (!reachedPoints.TryGetValue(dungeonName, out var points))
            {
                points = new HashSet<Vector2Int>();
                reachedPoints.Add(dungeonName, points);
            }

            if (points.Contains(point))
            {
                return;
            }

            points.Add(point);
            TinyServiceLocator.Resolve<GameEvents>().OnAddReachedPoint.OnNext((dungeonName, point));
        }

        public bool IsReachedPoint(string dungeonName, Vector2Int point)
        {
            if (!reachedPoints.TryGetValue(dungeonName, out var points))
            {
                return false;
            }

            return points.Contains(point);
        }

        public void AddExperience(int value)
        {
            experience.Value += value;
        }
    }
}
