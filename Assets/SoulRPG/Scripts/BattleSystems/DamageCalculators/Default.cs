using System;
using UnityEngine;
using UnitySequencerSystem;

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

        public int Calculate(BattleCharacter attacker, BattleCharacter defender, MasterData.Weapon attackerWeapon, Define.TargetType targetType, Container container)
        {
            var fixedAttackAttribute = attackAttribute.ToAttackAttribute(attackerWeapon);
            container.RegisterOrReplace("AttackAttribute", fixedAttackAttribute);
            var attackPower = attacker.BattleStatus.GetAttackPower(attackType);
            var weaponPower = attackerWeapon.Strength * weaponRate / 100.0f;
            var cutRate = defender.GetTotalCutRate(fixedAttackAttribute, attacker, container);
            var damage = (int)(attackPower * weaponPower * attacker.StatusBuffController.GetStrengthRate(attackType, attacker, defender, container) * (1.0f - cutRate));
            damage = Mathf.Max(1, damage);
            return damage;
        }
    }
}
