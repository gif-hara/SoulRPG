using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class BeginToBeContinued : ISequence
    {
        [SerializeField]
        private HKUIDocument documentPrefab;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            return ToBeContinuedView.OpenAsync(documentPrefab, cancellationToken);
        }
    }
}
