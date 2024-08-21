using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnitySequencerSystem;
using UnitySequencerSystem.Resolvers;

namespace SoulRPG
{
    /// <summary>
    /// 状態異常「効率化」によるBP削減ロジック
    /// </summary>
    [Serializable]
    public sealed class CalculateAilmentOptimizationLogic : ISequence
    {
        [SerializeReference, SubclassSelector]
        private IntResolver targetResolver;
        
        [SerializeReference, SubclassSelector]
        private StringResolver intNameResolver;

        [SerializeField]
        private int value;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var target = targetResolver.Resolve(container);
            if (target > 1)
            {
                target += value;
                container.RegisterOrReplace(intNameResolver.Resolve(container), target);
            }

            return UniTask.CompletedTask;
        }
    }
}
