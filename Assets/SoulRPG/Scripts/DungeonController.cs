using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.BattleSystems;
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

        private readonly HKUIDocument commandDocumentPrefab;

        private readonly CancellationToken scope;

        public DungeonController(HKUIDocument commandDocumentPrefab, CancellationToken scope)
        {
            this.commandDocumentPrefab = commandDocumentPrefab;
            this.scope = scope;
        }

        public void Setup(string dungeonName)
        {
            var masterData = TinyServiceLocator.Resolve<MasterData>();
            CurrentDungeon = masterData.Dungeons.Get(dungeonName);
        }

        public UniTask EnterAsync(Character character)
        {
            var masterData = TinyServiceLocator.Resolve<MasterData>();
            if (masterData.DungeonEvents.TryGetValue(character, out var dungeonEvent))
            {
                switch (dungeonEvent.EventType)
                {
                    case "Enemy":
                        return InvokeOnEnemyAsync(character, dungeonEvent);
                    default:
                        return UniTask.CompletedTask;
                }
            }
            else
            {
                return UniTask.CompletedTask;
            }
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
                        return UniTask.CompletedTask;
                }
            }
            else
            {
                Debug.Log("Not Found DungeonEvent");
                return UniTask.CompletedTask;
            }
        }

        private UniTask InvokeOnItemAsync(Character character, MasterData.DungeonEvent dungeonEvent)
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
            }
            TinyServiceLocator.Resolve<GameEvents>().OnAcquiredDungeonEvent.OnNext((CurrentDungeon.name, dungeonEvent.X, dungeonEvent.Y));
            userData.AddCompletedEventIds(dungeonEvent.Id, dungeonEvent.IsOneTime);
            return UniTask.CompletedTask;
        }

        private UniTask InvokeOnSavePointAsync(Character character, MasterData.DungeonEvent dungeonEvent)
        {
            TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext("ここはセーブポイントのようだ");
            var userData = TinyServiceLocator.Resolve<UserData>();
            userData.ClearTemporaryCompletedEventIds();
            return UniTask.CompletedTask;
        }

        private async UniTask InvokeOnEnemyAsync(Character character, MasterData.DungeonEvent dungeonEvent)
        {
            var masterDataEventEnemy = TinyServiceLocator.Resolve<MasterData>().DungeonEventEnemies.Get(dungeonEvent.Id);
            var masterDataEnemy = TinyServiceLocator.Resolve<MasterData>().Enemies.Get(masterDataEventEnemy.EnemyId);
            await BattleSystem.BeginAsync(
                new BattleCharacter(character, new Input(commandDocumentPrefab)),
                masterDataEnemy.CreateBattleCharacter(),
                scope
                );
        }
    }
}
