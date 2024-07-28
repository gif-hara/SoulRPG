using System.Collections.Generic;
using HK;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UserData
    {
        private readonly HashSet<string> completedEventIds = new();

        /// <summary>
        /// 一時的に完了したイベントのリスト
        /// </summary>
        /// <remarks>
        /// このイベントはセーブポイントにアクセスすると開放されて再度アクセス出来るようになります
        /// </remarks>
        private readonly HashSet<string> temporaryCompletedEventIds = new();

        public void AddCompletedEventIds(string eventId, bool isTemporary)
        {
            if (isTemporary)
            {
                temporaryCompletedEventIds.Add(eventId);
            }
            else
            {
                completedEventIds.Add(eventId);
            }
        }

        public bool ContainsCompletedEventId(string eventId)
        {
            return completedEventIds.Contains(eventId) || temporaryCompletedEventIds.Contains(eventId);
        }

        public void ClearTemporaryCompletedEventIds()
        {
            var tempData = new HashSet<string>(temporaryCompletedEventIds);
            temporaryCompletedEventIds.Clear();
            TinyServiceLocator.Resolve<GameEvents>().OnClearTemporaryCompletedEventIds.OnNext(tempData);
        }
    }
}
