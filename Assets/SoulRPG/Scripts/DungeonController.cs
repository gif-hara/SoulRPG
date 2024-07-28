using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.CharacterControllers;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class DungeonController
    {
        public MasterData.Dungeon CurrentDungeon { get; private set; }

        public void Setup(string dungeonName)
        {
            var masterData = TinyServiceLocator.Resolve<MasterData>();
            CurrentDungeon = masterData.Dungeons.Get(dungeonName);
        }

        public UniTask InteractAsync(Character character)
        {
            var masterData = TinyServiceLocator.Resolve<MasterData>();
            if (masterData.DungeonEvents.TryGetValue(character, out var dungeonEvent))
            {
                switch (dungeonEvent.EventType)
                {
                    case "Item":
                        return InvokeOnItemAsync(character, dungeonEvent);
                    case "SavePoint":
                        return InvokeOnSavePointAsync(character, dungeonEvent);
                    default:
                        Assert.IsTrue(false, $"Not Implement EventType {dungeonEvent.EventType}");
                        return UniTask.CompletedTask;
                }
            }
            else
            {
                Debug.Log("Not Found DungeonEvent");
                return UniTask.CompletedTask;
            }
        }

        public UniTask InvokeOnItemAsync(Character character, MasterData.DungeonEvent dungeonEvent)
        {
            var userData = TinyServiceLocator.Resolve<UserData>();
            if (userData.ContainsCompletedEventId(dungeonEvent.Id))
            {
                return UniTask.CompletedTask;
            }
            var masterDataEventItems = TinyServiceLocator.Resolve<MasterData>().DungeonEventItems.Get(dungeonEvent.Id);
            foreach (var item in masterDataEventItems)
            {
                character.Inventory.Add(item.ItemId, item.Count);
                TinyServiceLocator.Resolve<GameEvents>().OnAcquiredDungeonEvent.OnNext((CurrentDungeon.name, dungeonEvent.X, dungeonEvent.Y));
            }
            userData.AddCompletedEventIds(dungeonEvent.Id, dungeonEvent.IsOneTime);
            return UniTask.CompletedTask;
        }

        public UniTask InvokeOnSavePointAsync(Character character, MasterData.DungeonEvent dungeonEvent)
        {
            Debug.Log("SavePoint");
            var userData = TinyServiceLocator.Resolve<UserData>();
            userData.ClearTemporaryCompletedEventIds();
            return UniTask.CompletedTask;
        }
    }
}
