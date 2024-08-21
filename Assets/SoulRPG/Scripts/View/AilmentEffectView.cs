using System.Threading;
using Cysharp.Threading.Tasks;
using HK;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class AilmentEffectView
    {
        public static async UniTask OpenAsync(HKUIDocument documentPrefab, CancellationToken scope)
        {
            var document = UnityEngine.Object.Instantiate(documentPrefab);
            await scope.WaitUntilCanceled();
            UnityEngine.Object.Destroy(document.gameObject);
        }
    }
}
