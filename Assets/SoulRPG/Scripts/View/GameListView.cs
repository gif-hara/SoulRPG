using System.Collections.Generic;
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
    public sealed class GameListView
    {
        public class Element
        {
            public string header;

            public System.Action onClick;

            public System.Action onLeft;

            public System.Action onRight;
        }

        public static HKUIDocument Create(
        HKUIDocument listDocumentPrefab,
        IEnumerable<Element> listElements,
        int initialElement
        )
        {
            var document = Object.Instantiate(listDocumentPrefab);
            var listParent = document.Q<RectTransform>("Area.List");
            var listElementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            var inputController = TinyServiceLocator.Resolve<InputController>();
            var index = 0;
            foreach (var listElement in listElements)
            {
                var element = Object.Instantiate(listElementPrefab, listParent);
                var buttonObject = element.Q("Button");
                element.Q<TMP_Text>("Header").text = listElement.header;
                var button = element.Q<Button>("Button");
                button.OnClickAsObservable()
                    .Subscribe(_ =>
                    {
                        listElement.onClick();
                    })
                    .RegisterTo(element.destroyCancellationToken);
                var navigation = button.navigation;
                navigation.mode = Navigation.Mode.Vertical;
                navigation.wrapAround = true;
                button.navigation = navigation;
                inputController.InputActions.UI.Navigate.OnPerformedAsObservable()
                    .Where(x => x.ReadValue<Vector2>().x != 0)
                    .Where(_ => EventSystem.current.currentSelectedGameObject == buttonObject)
                    .Subscribe(x =>
                    {
                        if (x.ReadValue<Vector2>().x < 0)
                        {
                            listElement.onLeft?.Invoke();
                        }
                        else if (x.ReadValue<Vector2>().x > 0)
                        {
                            listElement.onRight?.Invoke();
                        }
                    })
                    .RegisterTo(element.destroyCancellationToken);
                if (index == initialElement)
                {
                    EventSystem.current.SetSelectedGameObject(button.gameObject);
                }
                index++;
            }
            return document;
        }
    }
}
