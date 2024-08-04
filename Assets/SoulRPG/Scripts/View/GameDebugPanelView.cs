using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using TMPro;
using UnityEngine;

namespace SoulRPG
{
#if DEBUG
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameDebugPanelView
    {
        public static async UniTask OpenAsync(HKUIDocument documentPrefab, CancellationToken scope)
        {
            var messages = new Dictionary<string, HKUIDocument>();
            var document = Object.Instantiate(documentPrefab);
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            var elementParent = document.Q<RectTransform>("ElementParent");
            gameEvents.OnRequestAddDebugPanelInformation
                .Subscribe(x =>
                {
                    if (!messages.ContainsKey(x.key))
                    {
                        var message = document.Q<HKUIDocument>("ElementPrefab");
                        var instance = Object.Instantiate(message, elementParent);
                        messages.Add(x.key, instance);
                    }
                    messages[x.key].Q<TMP_Text>("Text").text = x.message;
                })
                .RegisterTo(scope);
            await UniTask.WaitUntilCanceled(scope);
            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }
        }
    }
#endif
}
