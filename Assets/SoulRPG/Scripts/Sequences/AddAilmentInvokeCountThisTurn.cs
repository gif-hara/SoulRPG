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
    public sealed class AddAilmentInvokeCountThisTurn : ISequence
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeField]
        private int masterDataAilmentId;

        [SerializeField]
        private int value;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            container.TryResolve<BattleCharacter>("Actor", out var actor);
            container.TryResolve<BattleCharacter>("Target", out var target);
            if ((actor != null && actor.BattleStatus.IsDead) || (target != null && target.BattleStatus.IsDead))
            {
                return UniTask.CompletedTask;
            }
            var t = targetType == Define.TargetType.Self ? actor : target;
            t.AilmentController.AddAilmentInvokeCountThisTurn(masterDataAilmentId, value);
            return UniTask.CompletedTask;
        }
    }
}
