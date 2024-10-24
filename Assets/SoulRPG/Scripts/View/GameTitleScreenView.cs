using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using TMPro;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameTitleScreenView
    {
        public static async UniTask OpenAsync(HKUIDocument documentPrefab, CancellationToken scope)
        {
            var anyClickScope = new CancellationTokenSource();
            var document = Object.Instantiate(documentPrefab);
            var sequences = document.Q<HKUIDocument>("Sequences");
            var inputController = TinyServiceLocator.Resolve<InputController>();
            var saveData = SaveData.LoadSafe();
            var gameRule = TinyServiceLocator.Resolve<GameRule>();
            document.Q<CanvasGroup>("Area.Title").alpha = 0;
            var title = gameRule.GameTitle;
            title = title.Localized().Replace("{SealedName}", saveData.playerData.sealedName);
            document.Q<TMP_Text>("Text.Title").text = title;
            inputController.PushInputType(InputController.InputType.UI);
            sequences.Q<SequenceMonobehaviour>("Animation.AnyClick.Loop").PlayAsync(anyClickScope.Token).Forget();
            var anyClickText = document.Q<TMP_Text>("Text.AnyClick");
            document.Q("Text.Trial").SetActive(gameRule.IsTrialVersion);
            TinyServiceLocator.Resolve<InputScheme>().AnyChangedAsObservable()
                .Subscribe(_ =>
                {
                    anyClickText.text = inputController.InputActions.UI.Submit.GetTag() + ":開始".Localized();
                })
                .RegisterTo(scope);
            await inputController.InputActions.UI.Submit.OnPerformedAsObservable().FirstAsync();
            anyClickScope.Cancel();
            anyClickScope.Dispose();
            await sequences.Q<SequenceMonobehaviour>("Animation.In").PlayAsync(scope);
            inputController.PopInputType();
            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }
        }
    }
}
