using System;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG.SequenceSystems.Evaluates
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class Random : IEvaluate
    {
        [SerializeField]
        [Range(0, 1)]
        private float rate;

        public bool Evaluate(Container container)
        {
            return UnityEngine.Random.value < rate;
        }
    }
}
