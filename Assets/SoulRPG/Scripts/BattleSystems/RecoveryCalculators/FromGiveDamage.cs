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
            var damage = container.Resolve<int>("Damage");
            var recovery = Mathf.FloorToInt(damage * rate);
            recovery = Mathf.Max(1, recovery);
            return recovery;
        }
    }
}
