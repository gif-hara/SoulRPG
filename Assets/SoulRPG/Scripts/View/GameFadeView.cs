using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameFadeView
    {
        private readonly HKUIDocument document;

        public GameFadeView(HKUIDocument documentPrefab, CancellationToken scope)
        {
            document = Object.Instantiate(documentPrefab);
            document.Q<Image>("Image").color = new Color(0, 0, 0, 0);
            scope.Register(() =>
            {
                if (document == null)
                {
                    return;
                }
                Object.Destroy(document.gameObject);
            });
        }

        public UniTask BeginFadeAsyncInternal(Color from, Color to, float duration, CancellationToken scope)
        {
            return LMotion.Create(from, to, duration)
                .BindToColor(Image)
                .AddTo(document)
                .ToUniTask(scope);
        }

        public static UniTask BeginFadeAsync(Color from, Color to, float duration, CancellationToken scope)
        {
            var instance = TinyServiceLocator.Resolve<GameFadeView>();
            return instance.BeginFadeAsyncInternal(from, to, duration, scope);
        }

        public static UniTask BeginFadeAsync(Color to, float duration, CancellationToken scope)
        {
            var instance = TinyServiceLocator.Resolve<GameFadeView>();
            return instance.BeginFadeAsyncInternal(instance.Image.color, to, duration, scope);
        }

        private Image Image => document.Q<Image>("Image");
    }
}
