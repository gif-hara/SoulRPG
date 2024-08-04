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
            var elements = new List<HKUIDocument>();
            var activeIndex = 0;
            for (var i = 0; i < character.BattleStatus.BehaviorPointMaxReactiveProperty.CurrentValue; i++)
            {
                var element = Object.Instantiate(documentPrefab, elementParent);
                elements.Add(element);
                SetActiveElement(i, false);
            }
            character.BattleStatus.BehaviourPointReactiveProperty
                .Subscribe(x =>
                {
                    var min = x > activeIndex ? activeIndex : x;
                    var max = x > activeIndex ? x : activeIndex;
                    for (var i = min; i < max; i++)
                    {
                        SetActiveElement(i, x > activeIndex);
                    }
                })
                .RegisterTo(scope);

            await UniTask.WaitUntilCanceled(scope);

            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }

            void SetActiveElement(int index, bool isActive)
            {
                elements[index].Q("Active").SetActive(isActive);
                elements[index].Q("Deactive").SetActive(!isActive);
            }
        }
    }
}
