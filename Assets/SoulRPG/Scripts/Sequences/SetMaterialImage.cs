using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class SetMaterialImage : ISequence
    {
        [SerializeField]
        private Image target;

        [SerializeField]
        private Material material;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            target.material = material;
            return UniTask.CompletedTask;
        }
    }
}
