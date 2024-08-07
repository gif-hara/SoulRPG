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
    public sealed class ResetGuardPoint : ISequence
    {
        [SerializeField]
        private Define.TargetType targetType;
        
        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var key = targetType == Define.TargetType.Self ? "Actor" : "Target";
            var actor = container.Resolve<BattleCharacter>(key);
            actor.BattleStatus.ResetGuardPoint();
            return UniTask.CompletedTask;
        }
    }
}
