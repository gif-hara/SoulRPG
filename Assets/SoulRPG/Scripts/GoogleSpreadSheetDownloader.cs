using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GoogleSpreadSheetDownloader
    {
        const string url = "https://script.google.com/macros/s/AKfycbwyEVm1eT-QtPpS0HtXjGV9iI6GCMWDcpd4PmlEOShsgrhBfCGDNTwNDQdrVOlQGGz1oA/exec";

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
