using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ToBeContinuedView
    {
        public static UniTask OpenAsync(HKUIDocument documentPrefab, CancellationToken scope)
        {
            var document = Object.Instantiate(documentPrefab);
            return document
                .Q<HKUIDocument>("Sequences")
                .Q<SequenceMonobehaviour>("Animation.In")
                .PlayAsync(scope);
        }
    }
}
