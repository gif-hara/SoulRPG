using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using TMPro;
using UnityEngine;
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
            var source = new UniTaskCompletionSource<int>();
            var document = Object.Instantiate(documentPrefab);
            scope = CancellationTokenSource.CreateLinkedTokenSource(document.destroyCancellationToken, scope).Token;
            document.Q<TMP_Text>("Message").text = message;
            var elementParent = document.Q<Transform>("ElementParent");
            var elementPrefab = document.Q<HKUIDocument>("ElementPrefab");
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
                }
                var navigation = button.navigation;
                navigation.mode = Navigation.Mode.Horizontal;
                navigation.wrapAround = false;
                button.navigation = navigation;
            }
            var result = await source.Task;
            Object.Destroy(document.gameObject);
            return result;
        }
    }
}
