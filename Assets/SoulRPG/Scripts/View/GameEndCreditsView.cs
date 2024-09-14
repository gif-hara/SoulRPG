using System;
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
    public sealed class GameEndCreditsView
    {
        public static async UniTask OpenAsync(HKUIDocument documentPrefab, CancellationToken scope)
        {
            var document = UnityEngine.Object.Instantiate(documentPrefab);
            var sequences = document.Q<HKUIDocument>("Sequences");
            var skipText = document.Q<TMP_Text>("Text.Skip");
            var index = 0;
            var inputScope = new CancellationTokenSource();
            var inputController = TinyServiceLocator.Resolve<InputController>();
            var skipStateMachine = new TinyStateMachine();
            scope = CancellationTokenSource.CreateLinkedTokenSource(scope, inputScope.Token).Token;
            try
            {
                skipText.gameObject.SetActive(false);
                TinyServiceLocator.Resolve<InputScheme>().AnyChangedAsObservable()
                    .Subscribe(_ =>
                    {
                        skipText.text = inputController.InputActions.UI.Submit.GetTag() + " スキップ";
                    })
                    .RegisterTo(scope);
                skipStateMachine.Change(StateAcceptSkipAsync);
                while (true)
                {
                    var sequence = sequences.TryQ<SequenceMonobehaviour>($"Animation.In.{index}");
                    if (sequence == null)
                    {
                        break;
                    }
                    await sequence.PlayAsync(scope);
                    index++;
                }
            }
            finally
            {
                UnityEngine.Object.Destroy(document.gameObject);
            }

            UniTask StateAcceptSkipAsync(CancellationToken _scope)
            {
                inputController.InputActions.UI.Submit.OnPerformedAsObservable()
                    .Subscribe(_ =>
                    {
                        skipStateMachine.Change(StateConfirmSkipAsync);
                    })
                    .RegisterTo(_scope);
                return UniTask.CompletedTask;
            }

            async UniTask StateConfirmSkipAsync(CancellationToken _scope)
            {
                skipText.gameObject.SetActive(true);
                await UniTask.NextFrame(cancellationToken: _scope);
                inputController.InputActions.UI.Submit.OnPerformedAsObservable()
                    .Subscribe(_ =>
                    {
                        inputScope.Cancel();
                        inputScope.Dispose();
                    })
                    .RegisterTo(_scope);

                await UniTask.WhenAny
                (
                    UniTask.Delay(TimeSpan.FromSeconds(5.0f), cancellationToken: _scope),
                    UniTask.WaitUntilCanceled(_scope)
                );
                if (skipText != null)
                {
                    skipText.gameObject.SetActive(false);
                }
                skipStateMachine.Change(StateAcceptSkipAsync);
            }
        }
    }
}
