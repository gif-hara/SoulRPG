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
    public sealed class OpenTitleScreenView : ISequence
    {
        [SerializeField]
        private HKUIDocument documentPrefab;

        public async UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            await GameTitleScreenView.OpenAsync(documentPrefab, cancellationToken);
        }
    }
}
