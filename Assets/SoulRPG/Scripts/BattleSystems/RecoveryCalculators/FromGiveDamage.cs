using System;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG.BattleSystems.RecoveryCalculators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class FromGiveDamage : IRecoveryCalculator
    {
        [SerializeField]
        private float rate = 1.0f;

        public int Calculate(Container container)
        {
            if (!container.TryResolve<int>("Damage", out var damage))
            {
                return 0;
            }
            var recovery = Mathf.FloorToInt(damage * rate);
            recovery = Mathf.Max(1, recovery);
            return recovery;
        }
    }
}
