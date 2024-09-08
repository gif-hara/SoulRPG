using System.Collections.Generic;
using System.Linq;
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
    public sealed class DialogView
    {
        public static async UniTask<int> ConfirmAsync
        (
            HKUIDocument documentPrefab,
            string message,
            IEnumerable<string> options,
            int defaultIndex,
            CancellationToken scope
        )
        {
            var tempSelectObject = EventSystem.current.currentSelectedGameObject;
            var source = new UniTaskCompletionSource<int>();
            var document = Object.Instantiate(documentPrefab);
            scope = CancellationTokenSource.CreateLinkedTokenSource(document.destroyCancellationToken, scope).Token;
            document.Q<TMP_Text>("Message").text = message;
            var elementParent = document.Q<Transform>("ElementParent");
            var elementPrefab = document.Q<HKUIDocument>("ElementPrefab");
            var inputController = TinyServiceLocator.Resolve<InputController>();
            Selectable defaultSelectable = null;
            inputController.InputActions.UI.Navigate
                .OnPerformedAsObservable()
                .Subscribe(x =>
                {
                    if (EventSystem.current.currentSelectedGameObject != null || defaultSelectable == null)
                    {
                        return;
                    }
                    EventSystem.current.SetSelectedGameObject(defaultSelectable.gameObject);
                })
                .RegisterTo(document.destroyCancellationToken);

            foreach (var e in options.Select((x, i) => (x, i)))
            {
                var element = Object.Instantiate(elementPrefab, elementParent);
                var button = element.Q<Button>("Button");
                element.Q<TMP_Text>("Header").text = e.x;
                button.OnClickAsObservable()
                    .Subscribe(_ =>
                    {
                        source.TrySetResult(e.i);
                    })
                    .RegisterTo(scope);
                if (defaultIndex == e.i)
                {
                    button.Select();
                    defaultSelectable = button;
                }
                var navigation = button.navigation;
                navigation.mode = Navigation.Mode.Horizontal;
                navigation.wrapAround = false;
                button.navigation = navigation;
            }
            var result = await source.Task;
            EventSystem.current.SetSelectedGameObject(tempSelectObject);
            Object.Destroy(document.gameObject);
            return result;
        }
    }
}
