using System.Collections.Generic;
using HK;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameListView
    {
        public class Element
        {
            public string header;

            public System.Action<HKUIDocument> activateAction;

            public System.Action onClick;

            public System.Action onLeft;

            public System.Action onRight;

            public System.Action onSelected;
        }

        public static HKUIDocument Create(
        HKUIDocument listDocumentPrefab,
        IEnumerable<System.Action<HKUIDocument>> elementActivateActions,
        int initialElement
        )
        {
            var document = Object.Instantiate(listDocumentPrefab);
            var listParent = document.Q<RectTransform>("Area.List");
            var listElementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            var index = 0;
            foreach (var listElement in elementActivateActions)
            {
                var element = Object.Instantiate(listElementPrefab, listParent);
                var button = element.Q<Button>("Button");
                listElement(element);
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
