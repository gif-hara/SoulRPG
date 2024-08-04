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
    public sealed class RemoveStatusBuff : ISequence
    {
        [SerializeField]
        private string buffName;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var actor = container.Resolve<BattleCharacter>("Actor");
            actor.StatusBuffController.Remove(buffName);
            return UniTask.CompletedTask;
        }
    }
}
