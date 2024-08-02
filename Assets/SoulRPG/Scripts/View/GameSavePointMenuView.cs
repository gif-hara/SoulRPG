using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.CharacterControllers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameSavePointMenuView
    {
        private readonly HKUIDocument documentBundlePrefab;

        private readonly Character character;

        private readonly TinyStateMachine stateMachine;

        private readonly UniTaskCompletionSource openCompletionSource = new();

        private InputController inputController;

        public class ListElement
        {
            public string header;

            public System.Action onClick;

            public System.Action onLeft;

            public System.Action onRight;
        }

        public GameSavePointMenuView(HKUIDocument documentBundlePrefab, Character character)
        {
            this.documentBundlePrefab = documentBundlePrefab;
            this.character = character;
            this.stateMachine = new TinyStateMachine();
            inputController = TinyServiceLocator.Resolve<InputController>();
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
            var actions = TinyServiceLocator.Resolve<InputController>().InputActions.UI;
            var listDocument = CreateList(new List<System.Action<HKUIDocument>>
            {
                element =>
                {
                    GameListView.ApplyAsSimpleElement
                    (
                        element,
                        "レベルアップ",
                        _ =>
                        {
                        }
                    );
                },
            },
            0
            );

            inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(StateCloseAsync);
                })
                .RegisterTo(scope);
            await UniTask.WaitUntilCanceled(scope);
            Object.Destroy(listDocument.gameObject);
        }

        private UniTask StateCloseAsync(CancellationToken scope)
        {
            openCompletionSource.TrySetResult();
            return UniTask.CompletedTask;
        }

        private HKUIDocument CreateList
        (
            IEnumerable<System.Action<HKUIDocument>> elementActivateActions,
            int initialElement
        )
        {
            return GameListView.Create
            (
                documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.List"),
                elementActivateActions,
                initialElement
            );
        }
    }
}
