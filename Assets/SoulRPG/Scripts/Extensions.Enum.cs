using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        public static Vector2Int ToVector2Int(this Define.Direction self)
        {
            return self switch
            {
                Define.Direction.Up => new Vector2Int(0, 1),
                Define.Direction.Right => new Vector2Int(1, 0),
                Define.Direction.Down => new Vector2Int(0, -1),
                Define.Direction.Left => new Vector2Int(-1, 0),
                _ => throw new System.ArgumentOutOfRangeException()
            };
        }

        public static Define.Direction ToLeft(this Define.Direction self)
        {
            return self switch
            {
                Define.Direction.Up => Define.Direction.Left,
                Define.Direction.Right => Define.Direction.Up,
                Define.Direction.Down => Define.Direction.Right,
                Define.Direction.Left => Define.Direction.Down,
                _ => throw new System.ArgumentOutOfRangeException()
            };
        }

        public static Define.Direction ToRight(this Define.Direction self)
        {
            return self switch
            {
                Define.Direction.Up => Define.Direction.Right,
                Define.Direction.Right => Define.Direction.Down,
                Define.Direction.Down => Define.Direction.Left,
                Define.Direction.Left => Define.Direction.Up,
                _ => throw new System.ArgumentOutOfRangeException()
            };
        }

        public static Define.Direction ToOpposite(this Define.Direction self)
        {
            return self switch
            {
                Define.Direction.Up => Define.Direction.Down,
                Define.Direction.Right => Define.Direction.Left,
                Define.Direction.Down => Define.Direction.Up,
                Define.Direction.Left => Define.Direction.Right,
                _ => throw new System.ArgumentOutOfRangeException()
            };
        }

        public static Define.Direction Rotate(this Define.Direction self, Define.Direction direction)
        {
            return direction switch
            {
                Define.Direction.Up => self,
                Define.Direction.Right => self.ToRight(),
                Define.Direction.Down => self.ToOpposite(),
                Define.Direction.Left => self.ToLeft(),
                _ => throw new System.ArgumentOutOfRangeException()
            };
        }

        public static Vector2Int TransformVelocityByDirection(this Define.Direction self, Vector2Int velocity)
        {
            return self switch
            {
                Define.Direction.Up => new Vector2Int(velocity.x, velocity.y),
                Define.Direction.Right => new Vector2Int(velocity.y, -velocity.x),
                Define.Direction.Down => new Vector2Int(-velocity.x, -velocity.y),
                Define.Direction.Left => new Vector2Int(-velocity.y, velocity.x),
                _ => throw new System.ArgumentOutOfRangeException()
            };
        }

        public static float ToAngle(this Define.Direction self)
        {
            return self switch
            {
                Define.Direction.Up => 0.0f,
                Define.Direction.Right => 90.0f,
                Define.Direction.Down => 180.0f,
                Define.Direction.Left => 270.0f,
                _ => throw new System.ArgumentOutOfRangeException()
            };
        }

        public static Define.AttackAttribute ToAttackAttribute(this Define.WeaponAttackAttribute self, MasterData.Weapon weapon)
        {
            return self switch
            {
                Define.WeaponAttackAttribute.Weapon => weapon.AttackAttribute,
                Define.WeaponAttackAttribute.Slash => Define.AttackAttribute.Slash,
                Define.WeaponAttackAttribute.Blow => Define.AttackAttribute.Blow,
                Define.WeaponAttackAttribute.Thrust => Define.AttackAttribute.Thrust,
                Define.WeaponAttackAttribute.Magic => Define.AttackAttribute.Magic,
                Define.WeaponAttackAttribute.Fire => Define.AttackAttribute.Fire,
                Define.WeaponAttackAttribute.Thunder => Define.AttackAttribute.Thunder,
                _ => throw new System.ArgumentOutOfRangeException()
            };
        }

        public static (Vector2Int from, Vector2Int to) GetWallIndex(this Define.Direction self)
        {
            return self switch
            {
                Define.Direction.Up => (Vector2Int.zero, Vector2Int.right),
                Define.Direction.Right => (Vector2Int.right, new Vector2Int(1, -1)),
                Define.Direction.Down => (new Vector2Int(0, -1), new Vector2Int(1, -1)),
                Define.Direction.Left => (Vector2Int.zero, new Vector2Int(0, -1)),
                _ => throw new System.ArgumentOutOfRangeException()
            };
        }

        public static (Vector2Int from, Vector2Int to) GetWallPosition(this Define.Direction self, Vector2Int position)
        {
            var result = self.GetWallIndex();
            return (position + result.from, position + result.to);
        }
    }
}
