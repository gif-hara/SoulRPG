using System;
using System.Collections.Generic;
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
    public sealed class AddStatusBuff : ISequence
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeField]
        private List<Define.StatusType> statusTypes;

        [SerializeField]
        private string buffName;

        [SerializeField]
        private float rate;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var key = targetType == Define.TargetType.Self ? "Actor" : "Target";
            var actor = container.Resolve<BattleCharacter>(key);
            actor.StatusBuffController.Add(statusTypes, buffName, rate);
            return UniTask.CompletedTask;
        }
    }
}
