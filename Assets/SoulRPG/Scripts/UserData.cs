using System.Collections.Generic;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UserData
    {
        public HashSet<string> CompletedEventIds { get; } = new();

        /// <summary>
        /// 一時的に完了したイベントのリスト
        /// </summary>
        /// <remarks>
        /// このイベントはセーブポイントにアクセスすると開放されて再度アクセス出来るようになります
        /// </remarks>
        public HashSet<string> TemporaryCompletedEventIds { get; } = new();

        public void AddCompletedEventIds(string eventId, bool isTemporary)
        {
            if (isTemporary)
            {
                TemporaryCompletedEventIds.Add(eventId);
            }
            else
            {
                CompletedEventIds.Add(eventId);
            }
        }

        public bool ContainsCompletedEventId(string eventId)
        {
            return CompletedEventIds.Contains(eventId) || TemporaryCompletedEventIds.Contains(eventId);
        }

        public void ClearTemporaryCompletedEventIds()
        {
            TemporaryCompletedEventIds.Clear();
        }
    }
}
