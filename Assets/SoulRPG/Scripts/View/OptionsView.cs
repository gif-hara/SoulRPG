using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class OptionsView
    {
        public static UniTask OpenAsync(HKUIDocument documentPrefab, CancellationToken scope)
        {
            var document = Object.Instantiate(documentPrefab);
            scope.Register(() =>
            {
                if (document == null)
                {
                    return;
                }
                Object.Destroy(document.gameObject);
            });
            return UniTask.CompletedTask;
        }
    }
}
