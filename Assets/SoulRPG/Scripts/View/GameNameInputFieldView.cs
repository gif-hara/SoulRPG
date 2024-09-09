using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameNameInputFieldView
    {
        public static async UniTask<string> OpenAsync(HKUIDocument documentPrefab, CancellationToken scope)
        {
            var document = Object.Instantiate(documentPrefab);
            var source = new UniTaskCompletionSource<string>();

            document.Q<Button>("SubmitButton")
                .OnClickAsObservable()
                .Subscribe(_ =>
                {
                    source.TrySetResult(document.Q<TMP_InputField>("InputField").text);
                })
                .RegisterTo(document.destroyCancellationToken);
            
            var task = source.Task;
            var winIndex = await UniTask.WhenAny(UniTask.WaitUntilCanceled(scope), task);
            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }
            return winIndex == 0 ? "" : await task;
        }
    }
}
