using System;
using System.Collections.Generic;
using System.Linq;
using HK;
using R3;
using R3.Triggers;
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
        public static HKUIDocument Create
        (
            HKUIDocument listDocumentPrefab,
            IEnumerable<Action<HKUIDocument>> elementActivateActions,
            int initialElement
        )
        {
            var document = Object.Instantiate(listDocumentPrefab);
            var listParent = document.Q<RectTransform>("ListParent");
            var layoutGroup = document.Q<VerticalLayoutGroup>("ListParent");
            var listElementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            var parentSize = listParent.rect.height - layoutGroup.padding.top - layoutGroup.padding.bottom;
            var elementSize = ((RectTransform)listElementPrefab.transform).rect.height + layoutGroup.spacing;
            var elementCount = Mathf.FloorToInt(parentSize / elementSize);
            var pageIndex = 0;
            var pageMax = elementActivateActions.Count() / (elementCount + 1);
            var elementIndex = 0;
            var elements = new List<HKUIDocument>();
            CreateList(initialElement);

            void CreateList(int selectIndex)
            {
                foreach (var element in elements)
                {
                    Object.Destroy(element.gameObject);
                }
                elements.Clear();
                elementIndex = 0;
                foreach (var action in elementActivateActions.Skip(pageIndex * elementCount).Take(elementCount))
                {
                    var i = elementIndex;
                    var element = Object.Instantiate(listElementPrefab, listParent);
                    elements.Add(element);
                    var button = element.Q<Button>("Button");
                    action(element);
                    button.OnSelectAsObservable()
                        .Subscribe(_ =>
                        {
                            TinyServiceLocator.Resolve<InputController>().InputActions.UI.Navigate.OnPerformedAsObservable()
                                .TakeUntil(button.OnDeselectAsObservable())
                                .Subscribe(x =>
                                {
                                    if (pageMax == 0)
                                    {
                                        return;
                                    }
                                    var direction = x.ReadValue<Vector2>();
                                    if (direction.x == 0)
                                    {
                                        return;
                                    }
                                    if (direction.x > 0)
                                    {
                                        pageIndex = (pageIndex + 1) % (pageMax + 1);
                                        CreateList(0);
                                    }
                                    else if (direction.x < 0)
                                    {
                                        pageIndex = pageIndex == 0 ? pageMax : pageIndex - 1;
                                        CreateList(0);
                                    }
                                })
                                .RegisterTo(element.destroyCancellationToken);
                        })
                        .RegisterTo(element.destroyCancellationToken);
                    if (elementIndex == selectIndex)
                    {
                        EventSystem.current.SetSelectedGameObject(button.gameObject);
                    }
                    elementIndex++;
                }
                UpdatePage(pageIndex);
            }
            void UpdatePage(int index)
            {
                if (pageMax == 0)
                {
                    document.Q("Area.Page").SetActive(false);
                }
                else
                {
                    document.Q("Area.Page").SetActive(true);
                    document.Q<TMP_Text>("Text.Page").text = $"{index + 1}/{pageMax + 1}";
                }
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
