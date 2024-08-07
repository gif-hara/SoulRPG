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
    public sealed class AddGuardPoint : ISequence
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeField]
        private float rate;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var key = targetType == Define.TargetType.Self ? "Actor" : "Target";
            var actor = container.Resolve<BattleCharacter>(key);
            var max = actor.BattleStatus.HitPointMax / 2;
            actor.BattleStatus.AddGuardPoint(Mathf.FloorToInt(max * rate));
            return UniTask.CompletedTask;
        }
    }
}
