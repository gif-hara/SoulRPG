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
        private readonly Dictionary<string, HashSet<Vector2Int>> reachedPoints = new();

        private readonly ReactiveProperty<int> experience = new(0);
        public ReadOnlyReactiveProperty<int> Experience => experience;

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
