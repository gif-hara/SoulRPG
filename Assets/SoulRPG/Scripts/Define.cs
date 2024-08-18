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

        public enum StatusType
        {
            PhysicalStrength,
            MagicalStrength,
            SlashCutRate,
            BlowCutRate,
            ThrustCutRate,
            MagicCutRate,
            FireCutRate,
            ThunderCutRate,
        }

        public enum AilmentBehaviourType
        {
            OnAdded = 0,
            OnRemoved = 1,
            OnTurnEnd = 2,
            CanExecutableTurn = 3,
            OnComboFromGivedDamage = 4,
            OnComboFromTakedDamage = 5,
            CanAddAilment = 6,
            EvaluateEvade = 7,
            OnReset = 8,
            OnTakeDamage = 9,
            OnCalculateNeedBehaviourPoint = 11,
            OnBehaviourEnd = 12,
            OnTurnStart = 13,
        }

        public enum AllyType
        {
            Player,
            Enemy,
        }

        public enum GiveDamageType
        {
            /// <summary>
            /// 直接攻撃
            /// 通常の攻撃やスキルなどがこれに該当
            /// 回避出来る可能性がある
            /// この攻撃からコンボが発生する
            /// </summary>
            Direct,

            /// <summary>
            /// 間接攻撃
            /// 状態異常によるダメージやコンボなどがこれに該当
            /// 回避出来ない
            /// </summary>
            Indirect,
        }

        public const int HandWeaponId = 101998;

        public const int TestWeaponId = 101997;
    }
}
