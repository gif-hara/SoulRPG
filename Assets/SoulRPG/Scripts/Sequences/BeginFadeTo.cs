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
    public sealed class BeginFadeTo : ISequence
    {
        [SerializeField]
        private Color to;

        [SerializeField]
        private float duration;


        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            return GameFadeView.BeginFadeAsync(to, duration, cancellationToken);
        }
    }
}
