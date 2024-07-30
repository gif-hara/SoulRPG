using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using SoulRPG.BattleSystems;
using SoulRPG.CharacterControllers;
using UnityEngine;

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

        private Vector2Int checkPoint;

        public DungeonController(Vector2Int initialCheckPoint, HKUIDocument commandDocumentPrefab, CancellationToken scope)
        {
            checkPoint = initialCheckPoint;
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
            if (masterData.FloorEvents.TryGetValue(character, out var dungeonEvent))
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
            if (masterData.FloorEvents.TryGetValue(character, out var dungeonEvent))
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

        public bool IsExistWall(Vector2Int position, Define.Direction direction)
        {
            return CurrentDungeon.IsExistWall(position, direction);
        }

        public bool CanMove(Vector2Int position, Define.Direction direction)
        {
            return !IsExistWall(position, direction);
        }

        private UniTask InvokeOnItemAsync(Character character, MasterData.FloorEvent dungeonEvent)
        {
            var userData = TinyServiceLocator.Resolve<UserData>();
            if (userData.ContainsCompletedFloorEventId(dungeonEvent.Id))
            {
                return UniTask.CompletedTask;
            }
            var masterDataEventItems = TinyServiceLocator.Resolve<MasterData>().FloorEventItems.Get(dungeonEvent.Id);
            foreach (var item in masterDataEventItems)
            {
                character.Inventory.Add(item.ItemId, item.Count);
            }
            TinyServiceLocator.Resolve<GameEvents>().OnAcquiredDungeonEvent.OnNext((CurrentDungeon.name, dungeonEvent.X, dungeonEvent.Y));
            userData.AddCompletedfloorEventIds(dungeonEvent.Id, dungeonEvent.IsOneTime);
            return UniTask.CompletedTask;
        }

        private UniTask InvokeOnSavePointAsync(Character character, MasterData.FloorEvent dungeonEvent)
        {
            TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext("ここはセーブポイントのようだ。一休みしよう");
            var userData = TinyServiceLocator.Resolve<UserData>();
            userData.ClearTemporaryCompletedFloorEventIds();
            character.InstanceStatus.FullRecovery();
            checkPoint = character.Position;
            return UniTask.CompletedTask;
        }

        private async UniTask InvokeOnEnemyAsync(Character character, MasterData.FloorEvent dungeonEvent)
        {
            var userData = TinyServiceLocator.Resolve<UserData>();
            if (userData.ContainsCompletedFloorEventId(dungeonEvent.Id))
            {
                return;
            }

            var masterDataEventEnemy = TinyServiceLocator.Resolve<MasterData>().FloorEventEnemies.Get(dungeonEvent.Id);
            var masterDataEnemy = TinyServiceLocator.Resolve<MasterData>().Enemies.Get(masterDataEventEnemy.EnemyId);
            var battleResult = await BattleSystem.BeginAsync(
                new BattleCharacter(character, new Input(commandDocumentPrefab)),
                masterDataEnemy.CreateBattleCharacter(),
                scope
                );
            if (battleResult == Define.BattleResult.PlayerWin)
            {
                TinyServiceLocator.Resolve<GameEvents>().OnAcquiredDungeonEvent.OnNext((CurrentDungeon.name, dungeonEvent.X, dungeonEvent.Y));
                userData.AddCompletedfloorEventIds(dungeonEvent.Id, dungeonEvent.IsOneTime);
            }
            else
            {
                character.Warp(checkPoint);
                character.InstanceStatus.FullRecovery();
                TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext("どうやら安全な場所に移動されたようだ");
            }
        }
    }
}
