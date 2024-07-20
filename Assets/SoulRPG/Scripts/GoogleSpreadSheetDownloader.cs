using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GoogleSpreadSheetDownloader
    {
        const string url = "https://script.google.com/macros/s/AKfycbwkaCI4AsDmssprEoUoe9YAMUg6JyzMQmbzDCD6NsfcMC_GdGO_NAjcfQpWp-bl0YH89w/exec";

        public static async UniTask<string> DownloadAsync(string sheetName)
        {
            var request = UnityWebRequest.Get(url + "?sheetName=" + sheetName);
            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                // エラー処理
                UnityEngine.Debug.LogError(request.error);
                return null;
            }
            else
            {
                return request.downloadHandler.text;
            }
        }
    }
}
