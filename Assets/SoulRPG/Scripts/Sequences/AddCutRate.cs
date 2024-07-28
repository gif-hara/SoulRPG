using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoulRPG.BattleSystems.CommandInvokers;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class AddCutRate : ISequence
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeField]
        private Define.AttackAttribute attackAttribute;

        [SerializeField]
        private float cutRate;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var name = targetType == Define.TargetType.Self ? "Actor" : "Target";
            var target = container.Resolve<BattleCharacter>(name);
            if (target.BattleStatus.IsDead)
            {
                return UniTask.CompletedTask;
            }
            target.BattleStatus.AddCutRate(attackAttribute, cutRate);
            return UniTask.CompletedTask;
        }
    }
}
