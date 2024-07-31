using System.Collections.Generic;
using HK;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UserData
    {
        private readonly HashSet<string> completedFloorEventIds = new();

        /// <summary>
        /// 一時的に完了した床イベントのリスト
        /// </summary>
        /// <remarks>
        /// このイベントはセーブポイントにアクセスすると開放されて再度アクセス出来るようになります
        /// </remarks>
        private readonly HashSet<string> temporaryCompletedFloorEventIds = new();

        private readonly HashSet<string> completedWallEventIds = new();

        public void AddCompletedfloorEventIds(string eventId, bool isOneTime)
        {
            if (isOneTime)
            {
                completedFloorEventIds.Add(eventId);
            }
            else
            {
                temporaryCompletedFloorEventIds.Add(eventId);
            }
        }

        public bool ContainsCompletedFloorEventId(string eventId)
        {
            return completedFloorEventIds.Contains(eventId) || temporaryCompletedFloorEventIds.Contains(eventId);
        }

        public void ClearTemporaryCompletedFloorEventIds()
        {
            var tempData = new HashSet<string>(temporaryCompletedFloorEventIds);
            temporaryCompletedFloorEventIds.Clear();
            TinyServiceLocator.Resolve<GameEvents>().OnClearTemporaryCompletedEventIds.OnNext(tempData);
        }

        public void AddCompletedWallEventIds(string eventId)
        {
            completedWallEventIds.Add(eventId);
        }

        public bool ContainsCompletedWallEventId(string eventId)
        {
            return completedWallEventIds.Contains(eventId);
        }
    }
}
