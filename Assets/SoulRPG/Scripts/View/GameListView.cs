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
        public static HKUIDocument CreateWithPages
        (
            HKUIDocument listDocumentPrefab,
            IEnumerable<Action<HKUIDocument>> elementActivateActions,
            int initialElementIndex
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
            var pageMax = elementActivateActions.Count() / elementCount;
            if (elementActivateActions.Count() % elementCount == 0)
            {
                pageMax--;
            }
            var elementIndex = 0;
            var elements = new List<HKUIDocument>();
            var emptyArea = document.TryQ("Area.Empty");
            if (emptyArea != null)
            {
                emptyArea.SetActive(!elementActivateActions.Any());
            }
            Selectable defaultSelectable = null;
            var inputController = TinyServiceLocator.Resolve<InputController>();
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
            CreateList(initialElementIndex);

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
                            TinyServiceLocator.Resolve<GameEvents>().OnRequestPlaySfx.OnNext(new("Sfx.Select.0"));
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
                        defaultSelectable = button;
                    }
                    elementIndex++;
                }
                UpdatePage(pageIndex);
            }
            void UpdatePage(int index)
            {
                var pageArea = document.TryQ("Area.Page");
                if (pageArea == null)
                {
                    return;
                }
                if (pageMax == 0)
                {
                    pageArea.SetActive(false);
                }
                else
                {
                    pageArea.SetActive(true);
                    document.Q<TMP_Text>("Text.Page").text = $"{index + 1}/{pageMax + 1}";
                }
            }
            return document;
        }

        public static HKUIDocument CreateAsCommand
        (
            HKUIDocument listDocumentPrefab,
            IEnumerable<Action<HKUIDocument>> elementActivateActions,
            int initialElementIndex
        )
        {
            var document = Object.Instantiate(listDocumentPrefab);
            var listParent = document.Q<RectTransform>("ListParent");
            var listElementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            var elementIndex = 0;
            Selectable defaultSelectable = null;
            foreach (var action in elementActivateActions)
            {
                var element = Object.Instantiate(listElementPrefab, listParent);
                var button = element.Q<Button>("Button");
                action(element);
                button.OnSelectAsObservable()
                    .Subscribe(_ =>
                    {
                        TinyServiceLocator.Resolve<GameEvents>().OnRequestPlaySfx.OnNext(new("Sfx.Select.0"));
                    })
                    .RegisterTo(element.destroyCancellationToken);
                if (elementIndex == initialElementIndex)
                {
                    EventSystem.current.SetSelectedGameObject(button.gameObject);
                    defaultSelectable = button;
                }
                elementIndex++;
            }
            var inputController = TinyServiceLocator.Resolve<InputController>();
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
            return document;
        }

        public static void ApplyAsSimpleElement
        (
            HKUIDocument element,
            string header,
            Action<Unit> onClick,
            Action<BaseEventData> onSelect = null
        )
        {
            element.Q<TMP_Text>("Header").text = header;
            var button = element.Q<Button>("Button");
            button.OnClickAsObservable()
                .Subscribe(onClick)
                .RegisterTo(element.destroyCancellationToken);
            if (onSelect != null)
            {
                button.OnSelectAsObservable()
                    .Subscribe(onSelect)
                    .RegisterTo(element.destroyCancellationToken);
            }
        }
    }
}
