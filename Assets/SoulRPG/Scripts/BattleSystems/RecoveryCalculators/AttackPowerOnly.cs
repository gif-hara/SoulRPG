using System;
using UnityEngine;
using UnitySequencerSystem;

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
        private Define.TargetType targetType;

        public int Calculate(Container container)
        {
            container.TryResolve<BattleCharacter>("Actor", out var actor);
            container.TryResolve<BattleCharacter>("Target", out var target);
            var t = targetType == Define.TargetType.Self ? actor : target;
            var a = targetType == Define.TargetType.Self ? target : actor;
            var attackPower = t.BattleStatus.GetAttackPower(attackType) * rate;
            var recovery = (int)(attackPower * t.StatusBuffController.GetStrengthRate(attackType, t, a));
            recovery = Mathf.Max(1, recovery);
            return recovery;
        }
    }
}
