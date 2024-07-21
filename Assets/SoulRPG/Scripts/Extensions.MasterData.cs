using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        public static bool IsExistWall(this MasterData.Dungeon self, Vector2Int position, Define.Direction direction)
        {
            switch (direction)
            {
                case Define.Direction.Up:
                    return self.wall.ContainsKey((position, position + Vector2Int.right));
                case Define.Direction.Down:
                    return self.wall.ContainsKey((position + Vector2Int.down, position + Vector2Int.down + Vector2Int.right));
                case Define.Direction.Left:
                    return self.wall.ContainsKey((position, position + Vector2Int.down));
                case Define.Direction.Right:
                    return self.wall.ContainsKey((position + Vector2Int.right, position + Vector2Int.right + Vector2Int.down));
                default:
                    Debug.LogError($"Invalid direction: {direction}");
                    return false;
            }
        }
    }
}
