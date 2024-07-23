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
    public sealed class GameMenuView
    {
        private readonly HKUIDocument rootMenuDoumentPrefab;

        private readonly TinyStateMachine stateMachine;

        private readonly UniTaskCompletionSource openCompletionSource = new();

        public GameMenuView(HKUIDocument rootMenuDocumentPrefab)
        {
            this.rootMenuDoumentPrefab = rootMenuDocumentPrefab;
            this.stateMachine = new TinyStateMachine();
        }

        public async UniTask OpenAsync()
        {
            var inputController = TinyServiceLocator.Resolve<InputController>();
            inputController.ChangeInputType(InputController.InputType.UI);
            stateMachine.Change(StateRootMenuAsync);
            await openCompletionSource.Task;
            inputController.ChangeInputType(InputController.InputType.InGame);
        }

        private async UniTask StateRootMenuAsync(CancellationToken scope)
        {
            var document = Object.Instantiate(rootMenuDoumentPrefab);
            var inputController = TinyServiceLocator.Resolve<InputController>();
            var actions = inputController.InputActions.UI;
            actions.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(StateCloseAsync);
                })
                .RegisterTo(scope);
            await UniTask.WaitUntilCanceled(scope);
            Object.Destroy(document.gameObject);
        }

        private UniTask StateCloseAsync(CancellationToken scope)
        {
            openCompletionSource.TrySetResult();
            return UniTask.CompletedTask;
        }
    }
}
