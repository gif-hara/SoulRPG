using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using LitMotion;
using LitMotion.Extensions;
using R3;
using TMPro;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameTipsView
    {
        private readonly HKUIDocument document;

        private readonly TMP_Text message;

        private readonly RectTransform messageRectTransform;

        private readonly RectTransform messageParent;

        private readonly CanvasGroup canvasGroup;

        private readonly Stack<string> tips = new();

        private CancellationTokenSource animationScope = null;

        public GameTipsView(HKUIDocument documentPrefab, CancellationToken scope)
        {
            document = UnityEngine.Object.Instantiate(documentPrefab);
            message = document.Q<TMP_Text>("Message");
            messageRectTransform = message.GetComponent<RectTransform>();
            messageParent = message.transform.parent.GetComponent<RectTransform>();
            canvasGroup = document.Q<CanvasGroup>("CanvasGroup");
            document.gameObject.SetActive(false);
            scope.Register(() =>
            {
                if (document == null)
                {
                    return;
                }
                UnityEngine.Object.Destroy(document.gameObject);
            });
        }

        public static void PushTip(string tip)
        {
            var instance = TinyServiceLocator.Resolve<GameTipsView>();
            instance.tips.Push(tip);
            instance.UpdateTextAsync();
        }

        public void PopTip()
        {
            var instance = TinyServiceLocator.Resolve<GameTipsView>();
            instance.tips.Pop();
            instance.UpdateTextAsync();
        }

        private void UpdateTextAsync()
        {
            if (tips.Count == 0)
            {
                document.gameObject.SetActive(false);
                return;
            }
            document.gameObject.SetActive(true);
            message.text = tips.Peek();
            messageRectTransform.anchoredPosition = Vector2.zero;
            messageRectTransform.sizeDelta = new Vector2(message.preferredWidth, messageParent.sizeDelta.y);
            if (message.preferredWidth > messageParent.rect.width)
            {
                BeginSlideAnimationAsync();
            }
        }

        private async void BeginSlideAnimationAsync()
        {
            Debug.Log("BeginSlideAnimationAsync");
            animationScope?.Cancel();
            animationScope?.Dispose();
            animationScope = new CancellationTokenSource();
            while (true)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(3.0f), cancellationToken: animationScope.Token);
                await LMotion.Create(0.0f, -message.preferredWidth, message.preferredWidth / 100.0f)
                    .BindToAnchoredPositionX(messageRectTransform)
                    .ToUniTask(animationScope.Token);
                await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: animationScope.Token);
                await UniTask.WhenAll
                (
                    LMotion.Create(0.0f, 1.0f, 0.3f)
                        .BindToCanvasGroupAlpha(canvasGroup)
                        .ToUniTask(animationScope.Token),
                    LMotion.Create(20.0f, 0.0f, 0.3f)
                        .BindToAnchoredPositionX(messageRectTransform)
                        .ToUniTask(animationScope.Token)
                );
            }
        }
    }
}
