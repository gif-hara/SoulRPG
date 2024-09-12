using System;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG.BattleSystems.RecoveryCalculators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class Constant : IRecoveryCalculator
    {
        [SerializeField]
        private int value;

        public int Calculate(Container container)
        {
            return value;
        }
    }
}
