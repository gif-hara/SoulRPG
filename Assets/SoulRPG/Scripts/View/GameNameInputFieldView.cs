using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
            var inputField = document.Q<TMP_InputField>("InputField");
            var submitButton = document.Q<Button>("Button.Submit");

            submitButton
                .OnClickAsObservable()
                .Subscribe(_ =>
                {
                    source.TrySetResult(inputField.text);
                })
                .RegisterTo(document.destroyCancellationToken);
            inputField.onEndEdit.AsObservable()
                .Subscribe(_ =>
                {
                    EventSystem.current.SetSelectedGameObject(submitButton.gameObject);
                })
                .RegisterTo(document.destroyCancellationToken);
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
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
