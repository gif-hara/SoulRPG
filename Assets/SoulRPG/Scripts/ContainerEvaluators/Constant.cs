using System;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG.ContainerEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class Constant : IContainerEvaluator
    {
        [SerializeField]
        private bool value;

        public bool Evaluate(Container container)
        {
            return value;
        }
    }
}
