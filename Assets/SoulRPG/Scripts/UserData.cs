using System.Collections.Generic;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UserData
    {
        public HashSet<string> PermanentCompletedEventIds { get; } = new();

        public HashSet<string> TemporaryCompletedEventIds { get; } = new();

        public void AddCompletedEventIds(string eventId, bool isPermanent)
        {
            if (isPermanent)
            {
                PermanentCompletedEventIds.Add(eventId);
            }
            else
            {
                TemporaryCompletedEventIds.Add(eventId);
            }
        }

        public bool ContainsCompletedEventId(string eventId)
        {
            return PermanentCompletedEventIds.Contains(eventId) || TemporaryCompletedEventIds.Contains(eventId);
        }

        public void ClearTemporaryCompletedEventIds()
        {
            TemporaryCompletedEventIds.Clear();
        }
    }
}
