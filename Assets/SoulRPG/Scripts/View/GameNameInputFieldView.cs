using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnitySequencerSystem;

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
            var gameRule = TinyServiceLocator.Resolve<GameRule>();
            var saveData = SaveData.LoadSafe();
            var defaultPlayerName = saveData.playerData.sealedName == gameRule.DefaultSealedName.Localized()
                ? gameRule.DefaultPlayerNames[Random.Range(0, gameRule.DefaultPlayerNames.Count)]
                : saveData.playerData.sealedName;
            inputField.text = defaultPlayerName;

            submitButton
                .OnClickAsObservable()
                .Subscribe(async _ =>
                {
                    AudioManager.PlaySfx("Sfx.Message.0");
                    if (inputField.text.Length == 0)
                    {
                        await DialogView.ConfirmAsync(dialogDocumentPrefab, "名前を入力してください。".Localized(), new[] { "確認".Localized() }, 0, scope);
                        AudioManager.PlaySfx("Sfx.Message.0");
                        EventSystem.current.SetSelectedGameObject(inputField.gameObject);
                    }
                    else if (inputField.text.Length > 10)
                    {
                        await DialogView.ConfirmAsync(dialogDocumentPrefab, "名前は10文字以内で入力してください。".Localized(), new[] { "確認".Localized() }, 0, scope);
                        AudioManager.PlaySfx("Sfx.Message.0");
                        EventSystem.current.SetSelectedGameObject(inputField.gameObject);
                    }
                    else if (inputField.text == SaveData.LoadSafe().playerData.sealedName)
                    {
                        document.gameObject.SetActive(false);
                        var container = new Container();
                        var sequencer = new Sequencer(container, TinyServiceLocator.Resolve<GameRule>().SealedNameSubmitSequences);
                        await sequencer.PlayAsync(scope);
                        EventSystem.current.SetSelectedGameObject(inputField.gameObject);
                        document.gameObject.SetActive(true);
                    }
                    else
                    {
                        document.gameObject.SetActive(false);
                        var message = "「{0}」でよろしいですか？変更は出来ません。".Localized().Format(inputField.text);
                        var confirmIndex = await DialogView.ConfirmAsync(dialogDocumentPrefab, message, new[] { "はい".Localized(), "いいえ".Localized() }, 0, scope);
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
