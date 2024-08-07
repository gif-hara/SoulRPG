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

        private readonly HKUIDocument gameMenuBundlePrefab;

        private Vector2Int checkPoint;

        private readonly IExplorationView view;

        public DungeonController(
            Vector2Int initialCheckPoint,
            HKUIDocument gameMenuBundlePrefab,
            IExplorationView view
            )
        {
            checkPoint = initialCheckPoint;
            this.gameMenuBundlePrefab = gameMenuBundlePrefab;
            this.view = view;
        }

        public void Setup(string dungeonName)
        {
            var masterData = TinyServiceLocator.Resolve<MasterData>();
            CurrentDungeon = masterData.Dungeons.Get(dungeonName);
        }

        public UniTask EnterAsync(Character character)
        {
            AddReachedPoint(character);
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

        private void AddReachedPoint(Character character)
        {
            var userData = TinyServiceLocator.Resolve<UserData>();
            userData.AddReachedPoint(CurrentDungeon.name, character.Position);
            if (CanMove(character.Position, Define.Direction.Up))
            {
                userData.AddReachedPoint(CurrentDungeon.name, character.Position + Define.Direction.Up.ToVector2Int());
            }
            if (CanMove(character.Position, Define.Direction.Down))
            {
                userData.AddReachedPoint(CurrentDungeon.name, character.Position + Define.Direction.Down.ToVector2Int());
            }
            if (CanMove(character.Position, Define.Direction.Left))
            {
                userData.AddReachedPoint(CurrentDungeon.name, character.Position + Define.Direction.Left.ToVector2Int());
            }
            if (CanMove(character.Position, Define.Direction.Right))
            {
                userData.AddReachedPoint(CurrentDungeon.name, character.Position + Define.Direction.Right.ToVector2Int());
            }
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

        private async UniTask InvokeOnSavePointAsync(Character character, MasterData.FloorEvent floorEvent)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            await gameEvents.ShowMessageAndWaitForSubmitInputAsync("ここはセーブポイントのようだ。一休みしよう。");
            var userData = TinyServiceLocator.Resolve<UserData>();
            userData.ClearTemporaryCompletedFloorEventIds();
            character.InstanceStatus.FullRecovery();
            checkPoint = character.Position;
            var view = new GameSavePointMenuView(gameMenuBundlePrefab, character);
            await view.OpenAsync();
        }

        private async UniTask InvokeOnEnemyAsync(Character character, MasterData.FloorEvent floorEvent)
        {
            var userData = TinyServiceLocator.Resolve<UserData>();
            if (userData.ContainsCompletedFloorEventId(floorEvent.Id))
            {
                return;
            }
            var scope = new CancellationTokenSource();
            var masterDataEventEnemy = TinyServiceLocator.Resolve<MasterData>().FloorEventEnemies.Get(floorEvent.Id);
            var masterDataEnemy = TinyServiceLocator.Resolve<MasterData>().Enemies.Get(masterDataEventEnemy.EnemyId);
            var playerCharacter = new BattleCharacter(character, Define.AllyType.Player, new Input(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.Command")));
            var enemyCharacter = masterDataEnemy.CreateBattleCharacter();
            BehaviourPointView.OpenAsync(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.BehaviourPoint"), playerCharacter, scope.Token).Forget();
            var battleResult = await BattleSystem.BeginAsync(
                playerCharacter,
                enemyCharacter,
                scope.Token
                );
            character.InstanceStatus.ResetGuardPoint();
            if (battleResult == Define.BattleResult.PlayerWin)
            {
                TinyServiceLocator.Resolve<GameEvents>().OnAcquiredDungeonEvent.OnNext((CurrentDungeon.name, floorEvent.X, floorEvent.Y));
                userData.AddCompletedfloorEventIds(floorEvent.Id, floorEvent.IsOneTime);
                userData.AddExperience(masterDataEnemy.Experience);
            }
            else
            {
                character.Warp(checkPoint);
                character.InstanceStatus.FullRecovery();
                TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext("どうやら安全な場所に移動されたようだ");
            }
            scope.Cancel();
            scope.Dispose();
        }

        private async UniTask InvokeOnDoorAsync(Character character, MasterData.WallEvent wallEvent)
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
                        await view.OnOpenDoorAsync(wallEvent);
                    }
                    break;
                case "Lock":
                    if (!userData.ContainsCompletedWallEventId(wallEvent.Id))
                    {
                        TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext("こちらからは開かないようだ");
                    }
                    break;
                case "Item":
                    if (!userData.ContainsCompletedWallEventId(wallEvent.Id))
                    {
                        var masterDataWallEventConditionItems = TinyServiceLocator.Resolve<MasterData>().WallEventConditionItems.Get(wallEvent.Id);
                        foreach (var item in masterDataWallEventConditionItems)
                        {
                            if (!character.Inventory.HasItem(item))
                            {
                                TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext("鍵が必要のようだ");
                                return;
                            }
                        }
                        TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext("扉が開いた");
                        userData.AddCompletedWallEventIds(wallEvent.Id);
                        await view.OnOpenDoorAsync(wallEvent);
                    }
                    break;
            }
        }
    }
}
