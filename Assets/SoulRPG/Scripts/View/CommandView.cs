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
    public sealed class CommandView
    {
        private readonly HKUIDocument documentPrefab;

        private HKUIDocument document;

        private readonly List<GameObject> elements = new();

        public CommandView(HKUIDocument documentPrefab)
        {
            this.documentPrefab = documentPrefab;
        }

        public void Open()
        {
            document = Object.Instantiate(documentPrefab);
        }

        public void Close()
        {
            Object.Destroy(document.gameObject);
        }

        public UniTask<int> CreateCommandsAsync(
            string header,
            IEnumerable<string> commands,
            int initialIndex
            )
        {
            foreach (var e in elements)
            {
                Object.Destroy(e);
            }
            elements.Clear();
            var source = new UniTaskCompletionSource<int>();
            var listParent = document.Q<RectTransform>("ListParent");
            var listElementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            document.Q<TMP_Text>("Header").text = header;
            foreach (var (c, i) in commands.Select((c, i) => (c, i)))
            {
                var element = Object.Instantiate(listElementPrefab, listParent);
                elements.Add(element.gameObject);
                element.Q<TMP_Text>("Header").text = c;
                var button = element.Q<Button>("Button");
                button.OnClickAsObservable()
                    .Subscribe(_ =>
                    {
                        source.TrySetResult(i);
                    })
                    .RegisterTo(element.destroyCancellationToken);
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
