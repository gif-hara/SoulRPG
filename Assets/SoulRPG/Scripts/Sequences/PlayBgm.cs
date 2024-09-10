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
    public sealed class PlayBgm : ISequence
    {
        [SerializeField]
        private string bgmName;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            AudioManager.PlayBgm(bgmName);
            return UniTask.CompletedTask;
        }
    }
}
