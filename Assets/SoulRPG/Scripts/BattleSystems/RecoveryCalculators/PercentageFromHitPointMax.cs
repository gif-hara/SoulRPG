using System;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG.BattleSystems.RecoveryCalculators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class PercentageFromHitPointMax : IRecoveryCalculator
    {
        [SerializeField]
        private float rate = 1.0f;

        [SerializeField]
        private Define.TargetType targetType;

        public int Calculate(Container container)
        {
            container.TryResolve<BattleCharacter>("Actor", out var actor);
            container.TryResolve<BattleCharacter>("Target", out var target);
            var t = targetType == Define.TargetType.Self ? actor : target;
            return Mathf.FloorToInt(t.BattleStatus.HitPointMax * rate);
        }
    }
}
