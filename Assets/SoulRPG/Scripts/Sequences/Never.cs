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
    public sealed class Never : ISequence
    {
        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            return UniTask.Never(cancellationToken);
        }
    }
}
