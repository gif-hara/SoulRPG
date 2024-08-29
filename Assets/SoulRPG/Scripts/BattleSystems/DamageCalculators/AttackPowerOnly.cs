using System;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG.BattleSystems.DamageCalculators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class AttackPowerOnly : IDamageCalculator
    {
        [SerializeField]
        private float rate = 1.0f;

        [SerializeField]
        private Define.AttackType attackType;

        [SerializeField]
        private Define.AttackAttribute attackAttribute;

        public int Calculate(BattleCharacter attacker, BattleCharacter defender, MasterData.Weapon attackerWeapon, Define.TargetType targetType, Container container)
        {
            var attackPower = attacker.BattleStatus.GetAttackPower(attackType) * rate;
            var cutRate = defender.GetTotalCutRate(attackAttribute, attacker, container);
            var damage = (int)(attackPower * attacker.StatusBuffController.GetStrengthRate(attackType, attacker, defender, container) * (1.0f - cutRate));
            damage = Mathf.Max(1, damage);
            return damage;
        }
    }
}
