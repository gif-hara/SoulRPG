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
        public static async UniTask<string> OpenAsync
        (
            HKUIDocument nameInputfieldDocumentPrefab,
            HKUIDocument dialogDocumentPrefab,
            CancellationToken scope
        )
        {
            var document = Object.Instantiate(nameInputfieldDocumentPrefab);
            var source = new UniTaskCompletionSource<string>();
            var inputField = document.Q<TMP_InputField>("InputField");
            var submitButton = document.Q<Button>("Button.Submit");

            submitButton
                .OnClickAsObservable()
                .Subscribe(async _ =>
                {
                    AudioManager.PlaySfx("Sfx.Message.0");
                    if (inputField.text.Length == 0)
                    {
                        await DialogView.ConfirmAsync(dialogDocumentPrefab, "名前を入力してください。", new[] { "確認" }, 0, scope);
                        AudioManager.PlaySfx("Sfx.Message.0");
                        EventSystem.current.SetSelectedGameObject(inputField.gameObject);
                        return;
                    }
                    else if (inputField.text.Length > 10)
                    {
                        await DialogView.ConfirmAsync(dialogDocumentPrefab, "名前は10文字以内で入力してください。", new[] { "確認" }, 0, scope);
                        AudioManager.PlaySfx("Sfx.Message.0");
                        EventSystem.current.SetSelectedGameObject(inputField.gameObject);
                        return;
                    }
                    else
                    {
                        document.gameObject.SetActive(false);
                        var confirmIndex = await DialogView.ConfirmAsync(dialogDocumentPrefab, $"「{inputField.text}」でよろしいですか？変更は出来ません。", new[] { "はい", "いいえ" }, 0, scope);
                        AudioManager.PlaySfx("Sfx.Message.0");
                        if (confirmIndex == 0)
                        {
                            source.TrySetResult(inputField.text);
                        }
                        else
                        {
                            document.gameObject.SetActive(true);
                            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
                        }
                    }
                })
                .RegisterTo(document.destroyCancellationToken);
            inputField.onEndEdit.AsObservable()
                .Subscribe(_ =>
                {
                    AudioManager.PlaySfx("Sfx.Message.0");
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
