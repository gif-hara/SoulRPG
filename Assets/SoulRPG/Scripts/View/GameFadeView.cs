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
                Object.Destroy(document.gameObject);
            });
        }

        public UniTask BeginFadeAsync(Color from, Color to, float duration, CancellationToken scope)
        {
            return LMotion.Create(from, to, duration)
                .BindToColor(Image)
                .AddTo(document)
                .ToUniTask(scope);
        }

        public UniTask BeginFadeAsync(Color to, float duration, CancellationToken scope)
        {
            return BeginFadeAsync(Image.color, to, duration, scope);
        }

        private Image Image => document.Q<Image>("Image");
    }
}
