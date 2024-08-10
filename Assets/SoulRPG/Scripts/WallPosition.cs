using System;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public struct WallPosition : IEquatable<WallPosition>
    {
        public Vector2Int From;

        public Vector2Int To;

        public WallPosition(Vector2Int from, Vector2Int to)
        {
            From = from;
            To = to;
        }

        public WallPosition(int fromX, int fromY, int toX, int toY)
        {
            From = new Vector2Int(fromX, fromY);
            To = new Vector2Int(toX, toY);
        }

        public bool Equals(WallPosition other)
        {
            return From.Equals(other.From) && To.Equals(other.To);
        }

        public override bool Equals(object obj)
        {
            return obj is WallPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (From.GetHashCode() * 397) ^ To.GetHashCode();
            }
        }

        public static bool operator ==(WallPosition left, WallPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WallPosition left, WallPosition right)
        {
            return !left.Equals(right);
        }
    }
}
