using System.Collections.Generic;
using HK;
using SoulRPG.CharacterControllers;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Inventory
    {
        public Dictionary<int, int> Items { get; } = new();

        private readonly Character character;

        public Inventory(Character character)
        {
            this.character = character;
        }

        public void Add(int itemId, int count)
        {
            if (Items.ContainsKey(itemId))
            {
                Items[itemId] += count;
            }
            else
            {
                Items[itemId] = count;
            }
        }

        public void Remove(int itemId, int count)
        {
            if (Items.ContainsKey(itemId))
            {
                Items[itemId] -= count;
                if (Items[itemId] <= 0)
                {
                    Items.Remove(itemId);
                }
            }
        }

        public int GetNumber(int itemId)
        {
            return Items.ContainsKey(itemId) ? Items[itemId] : 0;
        }

        public bool HasItem(INeedItem needItem)
        {
            return GetNumber(needItem.ItemId) >= needItem.Count;
        }

        public void Sync(List<GameRule.InitialItemData> initialItemDatabase)
        {
            Items.Clear();
            foreach (var initialItem in initialItemDatabase)
            {
                Add(initialItem.ItemId, initialItem.Count);
            }
        }
    }
}
