using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnitySequencerSystem;
using UnitySequencerSystem.Resolvers;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class RegisterContainerBehaviourPoint : ISequence
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeReference, SubclassSelector]
        private StringResolver keyResolver;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var key = targetType == Define.TargetType.Self ? "Actor" : "Target";
            container.TryResolve<BattleCharacter>(key, out var actor);
            container.Register(keyResolver.Resolve(container), actor.BattleStatus.BehaviourPointReactiveProperty.CurrentValue);
            return UniTask.CompletedTask;
        }
    }
}
