using System;
using UnityEngine;

namespace SoulRPG.BattleSystems.DamageCalculators
{
    /// <summary>
    /// 通常のダメージ計算
    /// </summary>
    [Serializable]
    public sealed class Default : IDamageCalculator
    {
        [SerializeField]
        private float weaponRate = 1.0f;

        [SerializeField]
        private Define.AttackType attackType;

        [SerializeField]
        private Define.WeaponAttackAttribute attackAttribute;

        public int Calculate(BattleCharacter attacker, BattleCharacter defender, MasterData.Weapon attackerWeapon, Define.TargetType targetType)
        {
            var fixedAttackAttribute = attackAttribute.ToAttackAttribute(attackerWeapon);
            var attackPower = attacker.BattleStatus.GetAttackPower(attackType);
            var weaponPower = attackerWeapon.Strength * weaponRate / 100.0f;
            var cutRate = defender.GetTotalCutRate(fixedAttackAttribute);
            var damage = (int)(attackPower * weaponPower * attacker.StatusBuffController.GetStrengthRate(attackType) * (1.0f - cutRate));
            damage = Mathf.Max(1, damage);
            return damage;
        }
    }
}
