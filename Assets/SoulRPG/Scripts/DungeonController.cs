using System.Collections.Generic;
using System.Linq;
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

        public MasterData.DungeonSpec CurrentDungeonSpec { get; private set; }

        private readonly HKUIDocument gameMenuBundlePrefab;

        private Vector2Int checkPoint;

        private readonly IExplorationView view;

        public readonly Dictionary<Vector2Int, DungeonInstanceFloorData> FloorDatabase = new();

        public readonly Dictionary<WallPosition, DungeonInstanceWallData> WallDatabase = new();

        private readonly HashSet<Vector2Int> reachedPoints = new();

        public DungeonController(
            HKUIDocument gameMenuBundlePrefab,
            IExplorationView view
            )
        {
            this.gameMenuBundlePrefab = gameMenuBundlePrefab;
            this.view = view;
        }

        public void Setup(string dungeonName, Character player)
        {
            var masterData = TinyServiceLocator.Resolve<MasterData>();
            CurrentDungeon = masterData.Dungeons.Get(dungeonName);
            CurrentDungeonSpec = masterData.DungeonSpecs.Get(CurrentDungeon.name);
            var initialPosition = new Vector2Int(CurrentDungeonSpec.InitialX, CurrentDungeonSpec.InitialY);
            player.Warp(initialPosition);
            checkPoint = initialPosition;
            FloorDatabase.Clear();
            foreach (var floorEvent in masterData.FloorEvents.List.Where(x => x.DungeonName == dungeonName))
            {
                FloorDatabase.Add(new Vector2Int(floorEvent.X, floorEvent.Y), new DungeonInstanceFloorData(floorEvent));
            }
            WallDatabase.Clear();
            var masterDataWallEvents = masterData.WallEvents.List
                .Where(x => x.DungeonName == dungeonName);
            foreach (var wallEvent in masterDataWallEvents)
            {
                WallDatabase.Add(
                    wallEvent.GetWallPosition(),
                    new DungeonInstanceWallData(wallEvent)
                );
            }
        }

        public UniTask EnterAsync(Character character)
        {
            AddReachedPoint(character);
            if (FloorDatabase.TryGetValue(character.Position, out var floorData))
            {
                return floorData.EventType switch
                {
                    "Enemy" => InvokeOnEnemyAsync(character, floorData),
                    _ => UniTask.CompletedTask,
                };
            }
            else
            {
                return UniTask.CompletedTask;
            }
        }

        public UniTask InteractAsync(Character character)
        {
            var wallPosition = character.Direction.GetWallPosition(character.Position);
            if (FloorDatabase.TryGetValue(character.Position, out var floorData))
            {
                return floorData.EventType switch
                {
                    "Item" => InvokeOnItemAsync(character, floorData),
                    "SavePoint" => InvokeOnSavePointAsync(character),
                    _ => UniTask.CompletedTask,
                };
            }
            else if (WallDatabase.TryGetValue(wallPosition, out var wallEvent))
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

            if (WallDatabase.TryGetValue(direction.GetWallPosition(position), out var wallEvent))
            {
                return wallEvent.IsOpen;
            }

            return true;
        }

        private void AddReachedPoint(Character character)
        {
            Add(character.Position);
            if (CanMove(character.Position, Define.Direction.Up))
            {
                Add(character.Position + Define.Direction.Up.ToVector2Int());
            }
            if (CanMove(character.Position, Define.Direction.Down))
            {
                Add(character.Position + Define.Direction.Down.ToVector2Int());
            }
            if (CanMove(character.Position, Define.Direction.Left))
            {
                Add(character.Position + Define.Direction.Left.ToVector2Int());
            }
            if (CanMove(character.Position, Define.Direction.Right))
            {
                Add(character.Position + Define.Direction.Right.ToVector2Int());
            }

            void Add(Vector2Int position)
            {
                reachedPoints.Add(position);
                TinyServiceLocator.Resolve<GameEvents>().OnAddReachedPoint.OnNext(position);
            }
        }

        public bool ContainsReachedPoint(Vector2Int position)
        {
            return reachedPoints.Contains(position);
        }

        private UniTask InvokeOnItemAsync(Character character, DungeonInstanceFloorData floorData)
        {
            return UniTask.CompletedTask;
            // var masterDataEventItems = TinyServiceLocator.Resolve<MasterData>().FloorEventItems.Get(floorData.Id);
            // foreach (var item in masterDataEventItems)
            // {
            //     character.Inventory.Add(item.ItemId, item.Count);
            // }
            // TinyServiceLocator.Resolve<GameEvents>().OnAcquiredDungeonEvent.OnNext((CurrentDungeon.name, floorData.X, floorData.Y));
            // FloorDatabase.Remove(character.Position);
            // return UniTask.CompletedTask;
        }

        private async UniTask InvokeOnSavePointAsync(Character character)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            await gameEvents.ShowMessageAndWaitForSubmitInputAsync("ここはセーブポイントのようだ。一休みしよう。");
            character.InstanceStatus.FullRecovery();
            checkPoint = character.Position;
            var view = new GameSavePointMenuView(gameMenuBundlePrefab, character);
            await view.OpenAsync();
        }

        private UniTask InvokeOnEnemyAsync(Character character, DungeonInstanceFloorData floorData)
        {
            return UniTask.CompletedTask;
            // var scope = new CancellationTokenSource();
            // var masterDataEventEnemy = TinyServiceLocator.Resolve<MasterData>().FloorEventEnemies.Get(floorData.Id);
            // var masterDataEnemy = TinyServiceLocator.Resolve<MasterData>().Enemies.Get(masterDataEventEnemy.EnemyId);
            // var playerCharacter = new BattleCharacter(character, Define.AllyType.Player, new Input(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.Command")));
            // var enemyCharacter = masterDataEnemy.CreateBattleCharacter();
            // BehaviourPointView.OpenAsync(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.BehaviourPoint"), playerCharacter, scope.Token).Forget();
            // var battleResult = await BattleSystem.BeginAsync(
            //     playerCharacter,
            //     enemyCharacter,
            //     scope.Token
            //     );
            // character.InstanceStatus.ResetGuardPoint();
            // if (battleResult == Define.BattleResult.PlayerWin)
            // {
            //     TinyServiceLocator.Resolve<GameEvents>().OnAcquiredDungeonEvent.OnNext((CurrentDungeon.name, floorData.X, floorData.Y));
            //     FloorData.Remove(character.Position);
            // }
            // else
            // {
            //     character.Warp(checkPoint);
            //     character.InstanceStatus.FullRecovery();
            //     TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext("どうやら安全な場所に移動されたようだ");
            // }
            // scope.Cancel();
            // scope.Dispose();
        }

        private async UniTask InvokeOnDoorAsync(Character character, DungeonInstanceWallData wallEvent)
        {
            var isPositiveAccess = wallEvent.IsPositiveAccess(character.Direction);
            var condition = isPositiveAccess ? wallEvent.PositiveSideCondition : wallEvent.NegativeSideCondition;
            switch (condition)
            {
                case "None":
                    if (!wallEvent.IsOpen)
                    {
                        TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext("扉が開いた");
                        wallEvent.Open();
                        await view.OnOpenDoorAsync(wallEvent);
                    }
                    break;
                case "Lock":
                    if (!wallEvent.IsOpen)
                    {
                        TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext("こちらからは開かないようだ");
                    }
                    break;
                case "Item":
                    if (!wallEvent.IsOpen)
                    {
                        foreach (var i in wallEvent.NeedItems)
                        {
                            if (!character.Inventory.HasItem(i))
                            {
                                TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext("鍵が必要のようだ");
                                return;
                            }
                        }
                        TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext("扉が開いた");
                        wallEvent.Open();
                        await view.OnOpenDoorAsync(wallEvent);
                    }
                    break;
            }
        }
    }
}
