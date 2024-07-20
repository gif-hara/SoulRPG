using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        public static Define.Direction ToDirection(this Vector2Int self)
        {
            return (self.x, self.y) switch
            {
                (0, 1) => Define.Direction.Up,
                (1, 0) => Define.Direction.Right,
                (0, -1) => Define.Direction.Down,
                (-1, 0) => Define.Direction.Left,
                _ => throw new System.ArgumentOutOfRangeException(nameof(self), self, "Invalid direction vector"),
            };
        }

        public static bool CanConvertToDirection(this Vector2Int self)
        {
            return (self.x, self.y) switch
            {
                (0, 1) => true,
                (1, 0) => true,
                (0, -1) => true,
                (-1, 0) => true,
                _ => false,
            };
        }
    }
}
