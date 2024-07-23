using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameMenuView
    {
        private readonly HKUIDocument documentBundlePrefab;

        private readonly TinyStateMachine stateMachine;

        private readonly UniTaskCompletionSource openCompletionSource = new();

        private InputController inputController;

        public class ListElement
        {
            public string header;

            public System.Action onClick;
        }

        public GameMenuView(HKUIDocument documentBundlePrefab)
        {
            this.documentBundlePrefab = documentBundlePrefab;
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
            var listDocument = CreateListDocument(new List<ListElement>
            {
                new()
                {
                    header = "装備",
                    onClick = () =>
                    {
                        Debug.Log("装備");
                    }
                },
                new()
                {
                    header = "道具",
                    onClick = () =>
                    {
                        Debug.Log("道具");
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

        private HKUIDocument CreateListDocument(
            IEnumerable<ListElement> listElements,
            int initialElement
            )
        {
            var document = Object.Instantiate(documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.List"));
            var listParent = document.Q<RectTransform>("Area.List");
            var listElementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            var index = 0;
            foreach (var listElement in listElements)
            {
                var element = Object.Instantiate(listElementPrefab, listParent);
                element.Q<TMP_Text>("Header").text = listElement.header;
                element.Q<Button>("Button").OnClickAsObservable()
                    .Subscribe(_ =>
                    {
                        listElement.onClick();
                    })
                    .AddTo(element);
                if (index == initialElement)
                {
                    EventSystem.current.SetSelectedGameObject(element.Q("Button"));
                }
                index++;
            }
            return document;
        }
    }
}
