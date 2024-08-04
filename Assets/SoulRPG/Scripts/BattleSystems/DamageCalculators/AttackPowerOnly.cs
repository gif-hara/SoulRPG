using System;
using UnityEngine;

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

        public int Calculate(BattleCharacter attacker, BattleCharacter defender, MasterData.Weapon weapon)
        {
            var attackPower = attacker.BattleStatus.GetAttackPower(attackType) * rate;
            var cutRate = defender.GetTotalCutRate(attackAttribute);
            var damage = (int)(attackPower * attacker.StatusBuffController.GetStrengthRate(attackType) * (1.0f - cutRate));
            damage = Mathf.Max(1, damage);
            return damage;
        }
    }
}
