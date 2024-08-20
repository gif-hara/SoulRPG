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
    public sealed class CompareMagicCount : IContainerEvaluator
    {
        [SerializeField]
        private Define.TargetType targetType;
        
        [SerializeReference, SubclassSelector]
        private IntResolver valueResolver;

        [SerializeField]
        private IContainerEvaluator.CompareType compareType;
        
        public bool Evaluate(Container container)
        {
            var key = targetType == Define.TargetType.Self ? "Actor" : "Target";
            container.TryResolve<BattleCharacter>(key, out var actor);
            var magicCount = actor.BattleStatus.MagicCount;
            var value = valueResolver.Resolve(container);
            return compareType switch
            {
                IContainerEvaluator.CompareType.Equals => magicCount == value,
                IContainerEvaluator.CompareType.GreaterThan => magicCount > value,
                IContainerEvaluator.CompareType.GreaterThanOrEquals => magicCount >= value,
                IContainerEvaluator.CompareType.LessThan => magicCount < value,
                IContainerEvaluator.CompareType.LessThanOrEquals => magicCount <= value,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
