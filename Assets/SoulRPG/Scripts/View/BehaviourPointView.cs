using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class BehaviourPointView
    {
        public static async UniTask OpenAsync(HKUIDocument documentPrefab, BattleCharacter character, CancellationToken scope)
        {
            var document = Object.Instantiate(documentPrefab);
            var elementParent = document.Q<Transform>("ElementParent");
            var elementPrefab = document.Q<HKUIDocument>("ElementPrefab");
            var elements = new List<HKUIDocument>();
            var additinalElements = new List<HKUIDocument>();
            var activeIndex = 0;
            var behaviourPointMax = character.BattleStatus.BehaviorPointMaxReactiveProperty.CurrentValue;
            for (var i = 0; i < behaviourPointMax; i++)
            {
                var element = Object.Instantiate(elementPrefab, elementParent);
                elements.Add(element);
                SetActiveElement(element, false);
            }
            character.BattleStatus.BehaviourPointReactiveProperty
                .Subscribe(x =>
                {
                    var min = x > activeIndex ? activeIndex : x;
                    var max = x > activeIndex ? x : activeIndex;
                    for (var i = min; i < max; i++)
                    {
                        if (i < elements.Count)
                        {
                            SetActiveElement(elements[i], x > activeIndex);
                        }
                        else
                        {
                            if (x > activeIndex)
                            {
                                var element = Object.Instantiate(elementPrefab, elementParent);
                                additinalElements.Add(element);
                                SetActiveElement(element, x > activeIndex);
                            }
                            else
                            {
                                Object.Destroy(additinalElements[i - behaviourPointMax].gameObject);
                                additinalElements.RemoveAt(i - behaviourPointMax);
                            }
                        }
                    }
                    activeIndex = x;
                })
                .RegisterTo(scope);

            await UniTask.WaitUntilCanceled(scope);

            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }

            void SetActiveElement(HKUIDocument element, bool isActive)
            {
                element.Q("Active").SetActive(isActive);
                element.Q("Deactive").SetActive(!isActive);
            }
        }
    }
}
