using System;
using UnityEngine;

namespace SoulRPG.BattleSystems.DamageCalculators
{
    /// <summary>
    /// 通常のダメージ計算
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

        public int Calculate(BattleCharacter attacker, BattleCharacter defender, MasterData.Weapon weapon)
        {
            var attackPower = attacker.BattleStatus.GetAttackPower(attackType) * rate;
            var cutRate = defender.BattleStatus.GetCutRate(attackAttribute);
            var damage = (int)(attackPower * attacker.StrengthBuffController.GetRate(attackType) * (1.0f - cutRate));
            damage = Mathf.Max(1, damage);
            return damage;
        }
    }
}
