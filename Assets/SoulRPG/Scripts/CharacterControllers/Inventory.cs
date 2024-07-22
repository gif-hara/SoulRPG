using System.Collections.Generic;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Inventory
    {
        private readonly Dictionary<int, int> items = new();

        public void Add(int itemId, int count)
        {
            if (items.ContainsKey(itemId))
            {
                items[itemId] += count;
            }
            else
            {
                items[itemId] = count;
            }
        }

        public void Remove(int itemId, int count)
        {
            if (items.ContainsKey(itemId))
            {
                items[itemId] -= count;
                if (items[itemId] <= 0)
                {
                    items.Remove(itemId);
                }
            }
        }

        public int GetNumber(int itemId)
        {
            return items.ContainsKey(itemId) ? items[itemId] : 0;
        }
    }
}
