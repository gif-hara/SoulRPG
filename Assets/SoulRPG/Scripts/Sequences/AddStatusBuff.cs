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
    public sealed class AddStatusBuff : ISequence
    {
        [SerializeField]
        private Define.StatusType statusType;

        [SerializeField]
        private string buffName;

        [SerializeField]
        private float rate;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var actor = container.Resolve<BattleCharacter>("Actor");
            actor.StatusBuffController.Add(statusType, buffName, rate);
            return UniTask.CompletedTask;
        }
    }
}
