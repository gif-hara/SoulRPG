using System;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG.BattleSystems.DamageCalculators
{
    /// <summary>
    /// 固定ダメージ計算
    /// </summary>
    [Serializable]
    public sealed class Constant : IDamageCalculator
    {
        [SerializeField]
        private int damage;

        public int Calculate(BattleCharacter attacker, BattleCharacter defender, MasterData.Weapon attackerWeapon, Define.TargetType targetType, Container container)
        {
            return damage;
        }
    }
}
