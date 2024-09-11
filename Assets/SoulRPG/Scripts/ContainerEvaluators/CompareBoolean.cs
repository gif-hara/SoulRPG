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
    public sealed class CompareBoolean : IContainerEvaluator
    {
        [SerializeReference, SubclassSelector]
        private BooleanResolver targetResolver;

        [SerializeReference, SubclassSelector]
        private BooleanResolver valueResolver;

        public bool Evaluate(Container container)
        {
            var target = targetResolver.Resolve(container);
            var value = valueResolver.Resolve(container);
            return target == value;
        }
    }
}
