using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class FullRecoveryBehaviourPoint : ISequence
    {
        [SerializeField]
        private Define.TargetType targetType;
        
        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var key = targetType == Define.TargetType.Self ? "Actor" : "Target";
            if (!container.TryResolve<BattleCharacter>(key, out var actor))
            {
                var diff = actor.BattleStatus.BehaviorPointMaxReactiveProperty.CurrentValue -
                           actor.BattleStatus.BehaviourPointReactiveProperty.CurrentValue;
                actor.BattleStatus.AddBehaviourPoint(diff);
            }
            return UniTask.CompletedTask;
        }
    }
}
