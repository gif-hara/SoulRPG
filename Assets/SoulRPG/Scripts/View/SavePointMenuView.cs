using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.CharacterControllers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SavePointMenuView
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

        public SavePointMenuView(HKUIDocument documentBundlePrefab, Character character)
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
            var listDocument = CreateList(new List<GameListView.Element>
            {
                new()
                {
                    header = "装備",
                    onClick = () => Debug.Log("装備"),
                },
                new()
                {
                    header = "道具",
                    onClick = () =>
                    {
                        Debug.Log("道具");
                    },
                    onLeft = () =>
                    {
                        Debug.Log("左");
                    },
                    onRight = () =>
                    {
                        Debug.Log("右");
                    }
                },
                new()
                {
                    header = "ステータス",
                    onClick = () =>
                    {
                        Debug.Log("ステータス");
                    }
                },
                new()
                {
                    header = "システム",
                    onClick = () =>
                    {
                        Debug.Log("システム");
                    }
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
            IEnumerable<GameListView.Element> listElements,
            int initialElement
        )
        {
            return GameListView.Create
            (
                documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.List"),
                listElements,
                initialElement
            );
        }
    }
}
