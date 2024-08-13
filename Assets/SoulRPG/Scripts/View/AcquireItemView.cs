using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class AcquireItemView
    {
        public static async UniTask OpenAsync(HKUIDocument documentPrefab, CancellationToken scope)
        {
            var document = Object.Instantiate(documentPrefab);
            await scope.WaitUntilCanceled();
            Object.Destroy(document.gameObject);
        }
    }
}
