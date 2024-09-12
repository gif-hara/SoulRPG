using System;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG.BattleSystems.DamageCalculators
{
    /// <summary>
    /// 現在HPの割合のダメージ計算
    /// </summary>
    [Serializable]
    public sealed class PercentageFromHitPoint : IDamageCalculator
    {
        [SerializeField]
        private float rate;
        
        public int Calculate(BattleCharacter attacker, BattleCharacter defender, MasterData.Weapon attackerWeapon, Define.TargetType targetType, Container container)
        {
            var t = targetType == Define.TargetType.Self ? attacker : defender;
            return Mathf.FloorToInt(t.BattleStatus.HitPoint * rate);
        }
    }
}
