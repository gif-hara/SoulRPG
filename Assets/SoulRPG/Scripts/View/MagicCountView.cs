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
    public sealed class MagicCountView
    {
        public static async UniTask OpenAsync(HKUIDocument documentPrefab, BattleCharacter character, CancellationToken scope)
        {
            var document = Object.Instantiate(documentPrefab);
            var elementParent = document.Q<Transform>("ElementParent");
            var elementPrefab = document.Q<HKUIDocument>("ElementPrefab");
            var elements = new List<HKUIDocument>();
            character.BattleStatus.MagicCountReactiveProperty
                .Subscribe(x =>
                {
                    var diff = x - elements.Count;
                    if (diff > 0)
                    {
                        for (var i = 0; i < diff; i++)
                        {
                            var element = Object.Instantiate(elementPrefab, elementParent);
                            elements.Add(element);
                        }
                    }
                    else if (diff < 0)
                    {
                        for (var i = 0; i < -diff; i++)
                        {
                            Object.Destroy(elements[elements.Count - 1].gameObject);
                            elements.RemoveAt(elements.Count - 1);
                        }
                    }
                })
                .RegisterTo(scope);

            await UniTask.WaitUntilCanceled(scope);

            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }
        }
    }
}
