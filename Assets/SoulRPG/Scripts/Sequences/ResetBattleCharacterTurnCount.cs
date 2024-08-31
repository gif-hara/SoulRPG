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
    public sealed class ResetBattleCharacterTurnCount : ISequence
    {
        [SerializeField]
        private Define.TargetType targetType;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var actor = container.Resolve<BattleCharacter>("Actor");
            var target = container.Resolve<BattleCharacter>("Target");
            if (actor.BattleStatus.IsDead || target.BattleStatus.IsDead)
            {
                return UniTask.CompletedTask;
            }
            var t = targetType == Define.TargetType.Self ? actor : target;
            t.ResetTurnCount();
            return UniTask.CompletedTask;
        }
    }
}
