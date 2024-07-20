using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        public static Vector2Int ToVector2Int(this Vector2 self)
        {
            return new Vector2Int((int)self.x, (int)self.y);
        }
    }
}
