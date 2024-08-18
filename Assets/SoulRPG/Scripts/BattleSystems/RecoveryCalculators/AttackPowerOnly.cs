using System;
using UnityEngine;

namespace SoulRPG.BattleSystems.RecoveryCalculators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class AttackPowerOnly : IRecoveryCalculator
    {
        [SerializeField]
        private float rate = 1.0f;

        [SerializeField]
        private Define.AttackType attackType;

        [SerializeField]
        private Define.AttackAttribute attackAttribute;

        public int Calculate(BattleCharacter attacker, BattleCharacter defender, MasterData.Weapon attackerWeapon, Define.TargetType targetType)
        {
            var attackPower = attacker.BattleStatus.GetAttackPower(attackType) * rate;
            var recovery = (int)(attackPower * attacker.StatusBuffController.GetStrengthRate(attackType));
            recovery = Mathf.Max(1, recovery);
            return recovery;
        }
    }
}
