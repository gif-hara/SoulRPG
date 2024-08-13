using System;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG.SequenceSystems.Evaluates
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class Constant : IEvaluate
    {
        [SerializeField]
        private bool value;

        public bool Evaluate(Container container)
        {
            return value;
        }
    }
}
