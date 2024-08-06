using System;
using UnityEngine;
using UnitySequencerSystem;
using UnitySequencerSystem.Resolvers;

namespace SoulRPG.ContainerEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class Random : IContainerEvaluator
    {
        [SerializeReference, SubclassSelector]
        private FloatResolver probabilityResolver;

        public bool Evaluate(Container container)
        {
            var probability = probabilityResolver.Resolve(container);
            return UnityEngine.Random.value <= probability;
        }
    }
}
