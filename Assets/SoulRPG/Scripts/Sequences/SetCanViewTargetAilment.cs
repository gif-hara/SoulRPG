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
    public sealed class SetCanViewTargetAilment : ISequence
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeReference, SubclassSelector]
        private BooleanResolver flagResolver;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var t = targetType == Define.TargetType.Self ? "Actor" : "Target";
            var flag = flagResolver.Resolve(container);
            var target = container.Resolve<BattleCharacter>(t);
            target.BattleStatus.CanViewTargetAilment = flag;
            return UniTask.CompletedTask;
        }
    }
}
