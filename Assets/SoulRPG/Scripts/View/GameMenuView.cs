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

        public GameMenuView(HKUIDocument documentBundlePrefab)
        {
            this.documentBundlePrefab = documentBundlePrefab;
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
            var document = Object.Instantiate(documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.List"));
            var leftAreaDocument = document.Q<HKUIDocument>("Area.Left");
            var listParent = leftAreaDocument.Q<RectTransform>("Area.List");
            var listElementPrefab = leftAreaDocument.Q<HKUIDocument>("ListElementPrefab");
            var inputController = TinyServiceLocator.Resolve<InputController>();
            var actions = inputController.InputActions.UI;

            var firstElement = CreateListElement("装備");
            CreateListElement("道具");
            CreateListElement("ステータス");
            CreateListElement("システム");
            EventSystem.current.SetSelectedGameObject(firstElement.Q("Button"));

            actions.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(StateCloseAsync);
                })
                .RegisterTo(scope);
            await UniTask.WaitUntilCanceled(scope);
            Object.Destroy(document.gameObject);

            HKUIDocument CreateListElement(string header)
            {
                var listElement = Object.Instantiate(listElementPrefab, listParent);
                listElement.Q<TMP_Text>("Header").text = header;
                listElement.Q<Button>("Button").OnClickAsObservable()
                    .Subscribe(_ =>
                    {
                        Debug.Log(header);
                    })
                    .AddTo(listElement);
                return listElement;
            }
        }

        private UniTask StateCloseAsync(CancellationToken scope)
        {
            openCompletionSource.TrySetResult();
            return UniTask.CompletedTask;
        }
    }
}
