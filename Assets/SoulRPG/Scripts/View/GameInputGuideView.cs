using System;
using System.Threading;
using HK;
using R3;
using TMPro;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameInputGuideView
    {
        private readonly HKUIDocument document;

        public GameInputGuideView(HKUIDocument documentPrefab, CancellationToken scope)
        {
            document = UnityEngine.Object.Instantiate(documentPrefab);
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

        public void Open(Func<string> messageSelector, CancellationToken scope)
        {
            document.gameObject.SetActive(true);
            TinyServiceLocator.Resolve<InputScheme>().AnyChangedAsObservable()
                .Subscribe(_ =>
                {
                    document.Q<TMP_Text>("Message").text = messageSelector();
                })
                .RegisterTo(scope);
            scope.Register(() =>
            {
                document.gameObject.SetActive(false);
            });
        }
    }
}
