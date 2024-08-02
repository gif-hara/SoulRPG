using System;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public static class Define
    {
        public enum Direction
        {
            Up,
            Right,
            Down,
            Left,
        }

        public enum AttackAttribute
        {
            Slash,
            Blow,
            Thrust,
            Magic,
            Fire,
            Thunder,
        }

        public enum WeaponAttackAttribute
        {
            Weapon,
            Slash,
            Blow,
            Thrust,
            Magic,
            Fire,
            Thunder,
        }

        public enum BattleResult
        {
            PlayerWin,
            PlayerLose,
        }

        public enum AttackType
        {
            Physical,
            Magical,
        }

        public enum TargetType
        {
            Self,
            Opponent,
        }

        [Flags]
        public enum CharacterAttribute
        {
            /// <summary>なし</summary>
            None = 0,

            /// <summary>人型</summary>
            Humanoid = 1 << 0,

            /// <summary>不定形</summary>
            Formless = 1 << 1,
            
            /// <summary>鎧</summary>
            Armor = 1 << 2,
            
            /// <summary>甲羅</summary>
            Shell = 1 << 3,
        }

        public const int HandWeaponId = 101998;
    }
}
