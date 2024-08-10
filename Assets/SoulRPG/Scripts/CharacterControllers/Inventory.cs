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

            if (count == 1)
            {
                TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext(new($"{itemId.GetMasterDataItem().Name}を手に入れた。", "Sfx.Message.0"));
            }
            else
            {
                TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext(new($"{itemId.GetMasterDataItem().Name}を{count}個手に入れた。", "Sfx.Message.0"));
            }
            character.Events.OnAcquiredItem.OnNext((itemId, count));
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
    }
}
