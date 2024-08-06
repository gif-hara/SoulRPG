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
    public sealed class CompareInt : IContainerEvaluator
    {
        [SerializeField]
        private IntResolver targetResolver;
        
        [SerializeField]
        private IntResolver valueResolver;

        [SerializeField]
        private IContainerEvaluator.CompareType compareType;
        
        public bool Evaluate(Container container)
        {
            var target = targetResolver.Resolve(container);
            var value = valueResolver.Resolve(container);
            return compareType switch
            {
                IContainerEvaluator.CompareType.Equals => target == value,
                IContainerEvaluator.CompareType.NotEquals => target != value,
                IContainerEvaluator.CompareType.GreaterThan => target > value,
                IContainerEvaluator.CompareType.GreaterThanOrEquals => target >= value,
                IContainerEvaluator.CompareType.LessThan => target < value,
                IContainerEvaluator.CompareType.LessThanOrEquals => target <= value,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
