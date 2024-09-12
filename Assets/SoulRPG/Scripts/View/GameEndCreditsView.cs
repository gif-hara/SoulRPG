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
    public sealed class GameEndCreditsView
    {
        public static async UniTask OpenAsync(HKUIDocument documentPrefab, CancellationToken scope)
        {
            var document = Object.Instantiate(documentPrefab);
            var sequences = document.Q<HKUIDocument>("Sequences");
            var index = 0;
            var inputScope = new CancellationTokenSource();
            var inputController = TinyServiceLocator.Resolve<InputController>();
            scope = CancellationTokenSource.CreateLinkedTokenSource(scope, inputScope.Token).Token;
            try
            {
                inputController.InputActions.UI.Submit.OnPerformedAsObservable()
                    .Subscribe(_ =>
                    {
                        inputScope.Cancel();
                        inputScope.Dispose();
                    })
                    .RegisterTo(scope);
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
                Object.Destroy(document.gameObject);
            }
        }
    }
}
