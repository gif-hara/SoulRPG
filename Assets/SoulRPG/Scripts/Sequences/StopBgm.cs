using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class StopBgm : ISequence
    {
        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            AudioManager.StopBGM();
            return UniTask.CompletedTask;
        }
    }
}
