using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnitySequencerSystem;
using UnitySequencerSystem.Resolvers;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class FadeOutBgm : ISequence
    {
        [SerializeReference, SubclassSelector]
        private FloatResolver durationResolver;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            return AudioManager.FadeOutBgmAsync(durationResolver.Resolve(container), cancellationToken);
        }
    }
}
