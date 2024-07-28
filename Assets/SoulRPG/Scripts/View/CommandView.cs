using System.Collections.Generic;
using System.Linq;
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
    public sealed class CommandView
    {
        private readonly HKUIDocument documentPrefab;

        private HKUIDocument document;

        public CommandView(HKUIDocument documentPrefab)
        {
            this.documentPrefab = documentPrefab;
        }

        public void Open()
        {
            document = Object.Instantiate(documentPrefab);
        }

        public UniTask<int> CreateCommandsAsync(IEnumerable<string> commands, int initialIndex)
        {
            var source = new UniTaskCompletionSource<int>();
            var listParent = document.Q<RectTransform>("ListParent");
            var listElementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            foreach (var (c, i) in commands.Select((c, i) => (c, i)))
            {
                var element = Object.Instantiate(listElementPrefab, listParent);
                element.Q<TMP_Text>("Header").text = c;
                var button = element.Q<Button>("Button");
                button.OnClickAsObservable()
                    .Subscribe(_ =>
                    {
                        source.TrySetResult(i);
                    })
                    .AddTo(element);
                var navigation = button.navigation;
                navigation.mode = Navigation.Mode.Automatic;
                button.navigation = navigation;
                if (i == initialIndex)
                {
                    EventSystem.current.SetSelectedGameObject(button.gameObject);
                }
            }

            return source.Task;
        }
    }
}
