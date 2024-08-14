using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class AcquireItemView
    {
        public static async UniTask OpenAsync(HKUIDocument documentPrefab, MasterData.Item masterDataItem, CancellationToken scope)
        {
            var document = Object.Instantiate(documentPrefab);
            document.Q<Image>("Image").sprite = masterDataItem.Thumbnail;
            document.Q<TMP_Text>("Name").text = masterDataItem.Name;
            await scope.WaitUntilCanceled();
            Object.Destroy(document.gameObject);
        }
    }
}
