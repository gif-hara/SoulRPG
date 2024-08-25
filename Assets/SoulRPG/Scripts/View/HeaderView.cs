using System.Threading;
using HK;
using TMPro;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class HeaderView
    {
        public static HKUIDocument Open(HKUIDocument documentPrefab, string header, CancellationToken scope)
        {
            var document = Object.Instantiate(documentPrefab);
            document.Q<TMP_Text>("Header").text = header;
            scope.Register(() =>
            {
                if (document == null)
                {
                    return;
                }
                Object.Destroy(document.gameObject);
            });
            return document;
        }
    }
}
