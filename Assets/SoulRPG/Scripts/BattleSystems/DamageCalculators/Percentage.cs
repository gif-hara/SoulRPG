using System;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG.BattleSystems.DamageCalculators
{
    /// <summary>
    /// 割合ダメージ計算
    /// </summary>
    [Serializable]
    public sealed class Percentage : IDamageCalculator
    {
        [SerializeField]
        private float rate = 1.0f;

        public int Calculate(BattleCharacter attacker, BattleCharacter defender, MasterData.Weapon attackerWeapon, Define.TargetType targetType, Container container)
        {
            var t = targetType == Define.TargetType.Self ? attacker : defender;
            return Mathf.FloorToInt(t.BattleStatus.HitPointMax * rate);
        }
    }
}
