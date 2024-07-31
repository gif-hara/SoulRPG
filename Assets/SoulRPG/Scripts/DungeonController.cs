using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
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
                return dungeonEvent.EventType switch
                {
                    "Item" => InvokeOnItemAsync(character, dungeonEvent),
                    "SavePoint" => InvokeOnSavePointAsync(character, dungeonEvent),
                    _ => UniTask.CompletedTask,
                };
            }
            else if (masterData.WallEvents.TryGetValue(character, out var wallEvent))
            {
                return wallEvent.EventType switch
                {
                    "Door" => InvokeOnDoorAsync(character, wallEvent),
                    _ => UniTask.CompletedTask,
                };
            }
            else
            {
                return UniTask.CompletedTask;
            }
        }

        public bool IsExistWall(Vector2Int position, Define.Direction direction)
        {
            return CurrentDungeon.IsExistWall(position, direction);
        }

        public bool CanMove(Vector2Int position, Define.Direction direction)
        {
            if (IsExistWall(position, direction))
            {
                return false;
            }

            var masterData = TinyServiceLocator.Resolve<MasterData>();
            var userData = TinyServiceLocator.Resolve<UserData>();
            if (masterData.WallEvents.TryGetValue(position, direction, out var wallEvent))
            {
                return wallEvent.EventType switch
                {
                    "Door" => userData.ContainsCompletedWallEventId(wallEvent.Id),
                    _ => false,
                };
            }

            return true;
        }

        private UniTask InvokeOnItemAsync(Character character, MasterData.FloorEvent floorEvent)
        {
            var userData = TinyServiceLocator.Resolve<UserData>();
            if (userData.ContainsCompletedFloorEventId(floorEvent.Id))
            {
                return UniTask.CompletedTask;
            }
            var masterDataEventItems = TinyServiceLocator.Resolve<MasterData>().FloorEventItems.Get(floorEvent.Id);
            foreach (var item in masterDataEventItems)
            {
                character.Inventory.Add(item.ItemId, item.Count);
            }
            TinyServiceLocator.Resolve<GameEvents>().OnAcquiredDungeonEvent.OnNext((CurrentDungeon.name, floorEvent.X, floorEvent.Y));
            userData.AddCompletedfloorEventIds(floorEvent.Id, floorEvent.IsOneTime);
            return UniTask.CompletedTask;
        }

        private UniTask InvokeOnSavePointAsync(Character character, MasterData.FloorEvent floorEvent)
        {
            TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext("ここはセーブポイントのようだ。一休みしよう");
            var userData = TinyServiceLocator.Resolve<UserData>();
            userData.ClearTemporaryCompletedFloorEventIds();
            character.InstanceStatus.FullRecovery();
            checkPoint = character.Position;
            return UniTask.CompletedTask;
        }

        private async UniTask InvokeOnEnemyAsync(Character character, MasterData.FloorEvent floorEvent)
        {
            var userData = TinyServiceLocator.Resolve<UserData>();
            if (userData.ContainsCompletedFloorEventId(floorEvent.Id))
            {
                return;
            }

            var masterDataEventEnemy = TinyServiceLocator.Resolve<MasterData>().FloorEventEnemies.Get(floorEvent.Id);
            var masterDataEnemy = TinyServiceLocator.Resolve<MasterData>().Enemies.Get(masterDataEventEnemy.EnemyId);
            var battleResult = await BattleSystem.BeginAsync(
                new BattleCharacter(character, new Input(commandDocumentPrefab)),
                masterDataEnemy.CreateBattleCharacter(),
                scope
                );
            if (battleResult == Define.BattleResult.PlayerWin)
            {
                TinyServiceLocator.Resolve<GameEvents>().OnAcquiredDungeonEvent.OnNext((CurrentDungeon.name, floorEvent.X, floorEvent.Y));
                userData.AddCompletedfloorEventIds(floorEvent.Id, floorEvent.IsOneTime);
            }
            else
            {
                character.Warp(checkPoint);
                character.InstanceStatus.FullRecovery();
                TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext("どうやら安全な場所に移動されたようだ");
            }
        }

        private UniTask InvokeOnDoorAsync(Character character, MasterData.WallEvent wallEvent)
        {
            var userData = TinyServiceLocator.Resolve<UserData>();
            var isPositiveAccess = wallEvent.IsPositiveAccess(character.Direction);
            var condition = isPositiveAccess ? wallEvent.PositiveSideCondition : wallEvent.NegativeSideCondition;
            switch (condition)
            {
                case "None":
                    if (!userData.ContainsCompletedWallEventId(wallEvent.Id))
                    {
                        TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext("扉が開いた");
                        userData.AddCompletedWallEventIds(wallEvent.Id);
                    }
                    break;
                case "Lock":
                    if (!userData.ContainsCompletedWallEventId(wallEvent.Id))
                    {
                        TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext("こちらからは開かないようだ");
                    }
                    break;
            }

            return UniTask.CompletedTask;
        }
    }
}
