using System;
using System.Collections.Generic;
using HK;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameListView
    {
        public static HKUIDocument Create(
        HKUIDocument listDocumentPrefab,
        IEnumerable<Action<HKUIDocument>> elementActivateActions,
        int initialElement
        )
        {
            var document = Object.Instantiate(listDocumentPrefab);
            var listParent = document.Q<RectTransform>("Area.List");
            var listElementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            var index = 0;
            foreach (var action in elementActivateActions)
            {
                var element = Object.Instantiate(listElementPrefab, listParent);
                action(element);
                if (index == initialElement)
                {
                    EventSystem.current.SetSelectedGameObject(element.Q("Button"));
                }
                index++;
            }
            return document;
        }

        public static void ApplyAsSimpleElement(HKUIDocument element, string header, Action<Unit> onClick)
        {
            element.Q<TMP_Text>("Header").text = header;
            var button = element.Q<Button>("Button");
            button.OnClickAsObservable()
                .Subscribe(onClick)
                .RegisterTo(element.destroyCancellationToken);
        }
    }
}
