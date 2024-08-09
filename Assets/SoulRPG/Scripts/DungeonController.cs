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

        private readonly Dictionary<Vector2Int, MasterData.FloorEvent> floorEvents = new();

        public readonly Dictionary<(Vector2Int from, Vector2Int to), DungeonInstanceWallData> wallData = new();
        
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
            floorEvents.Clear();
            foreach (var floorEvent in masterData.FloorEvents.List.Where(x => x.DungeonName == dungeonName))
            {
                floorEvents.Add(new Vector2Int(floorEvent.X, floorEvent.Y), floorEvent);
            }
            wallData.Clear();
            var masterDataWallEvents = masterData.WallEvents.List
                .Where(x => x.DungeonName == dungeonName);
            foreach (var wallEvent in masterDataWallEvents)
            {
                wallData.Add(
                    (new Vector2Int(wallEvent.LeftX, wallEvent.LeftY), new Vector2Int(wallEvent.RightX, wallEvent.RightY)),
                    new DungeonInstanceWallData(wallEvent)
                );
            }
        }

        public UniTask EnterAsync(Character character)
        {
            AddReachedPoint(character);
            if (floorEvents.TryGetValue(character.Position, out var dungeonEvent))
            {
                return dungeonEvent.EventType switch
                {
                    "Enemy" => InvokeOnEnemyAsync(character, dungeonEvent),
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
            var wallPositions = character.Direction.GetWallPosition(character.Position);
            if (floorEvents.TryGetValue(character.Position, out var dungeonEvent))
            {
                return dungeonEvent.EventType switch
                {
                    "Item" => InvokeOnItemAsync(character, dungeonEvent),
                    "SavePoint" => InvokeOnSavePointAsync(character, dungeonEvent),
                    _ => UniTask.CompletedTask,
                };
            }
            else if (wallData.TryGetValue(wallPositions, out var wallEvent))
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

            var key = direction.GetWallPosition(position);
            if (wallData.TryGetValue(key, out var wallEvent))
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

        private UniTask InvokeOnItemAsync(Character character, MasterData.FloorEvent floorEvent)
        {
            var masterDataEventItems = TinyServiceLocator.Resolve<MasterData>().FloorEventItems.Get(floorEvent.Id);
            foreach (var item in masterDataEventItems)
            {
                character.Inventory.Add(item.ItemId, item.Count);
            }
            TinyServiceLocator.Resolve<GameEvents>().OnAcquiredDungeonEvent.OnNext((CurrentDungeon.name, floorEvent.X, floorEvent.Y));
            floorEvents.Remove(character.Position);
            return UniTask.CompletedTask;
        }

        private async UniTask InvokeOnSavePointAsync(Character character, MasterData.FloorEvent floorEvent)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            await gameEvents.ShowMessageAndWaitForSubmitInputAsync("ここはセーブポイントのようだ。一休みしよう。");
            character.InstanceStatus.FullRecovery();
            checkPoint = character.Position;
            var view = new GameSavePointMenuView(gameMenuBundlePrefab, character);
            await view.OpenAsync();
        }

        private async UniTask InvokeOnEnemyAsync(Character character, MasterData.FloorEvent floorEvent)
        {
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
                floorEvents.Remove(character.Position);
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
