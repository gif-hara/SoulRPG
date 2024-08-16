using System.Threading;
using HK;
using TMPro;
using UnityEngine;

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
            document = Object.Instantiate(documentPrefab);
            document.gameObject.SetActive(false);
            scope.Register(() =>
            {
                Object.Destroy(document.gameObject);
            });
        }

        public void Open(string message)
        {
            document.gameObject.SetActive(true);
            document.Q<TMP_Text>("Message").text = message;
        }
    }
}
