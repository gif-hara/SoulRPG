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
            gameEvents.OnRequestAddDebugPanelInformation
                .Subscribe(x =>
                {
                    if (!messages.ContainsKey(x.key))
                    {
                        var instance = Object.Instantiate(document);
                        instance.GetComponentInChildren<TextMeshProUGUI>().text = x.message;
                        messages.Add(x.key, instance);
                    }
                    messages[x.key].Q<TMP_Text>("Text").text = x.message;
                })
                .RegisterTo(scope);
            await UniTask.WaitUntilCanceled(scope);
            Object.Destroy(document.gameObject);
        }
    }
#endif
}
