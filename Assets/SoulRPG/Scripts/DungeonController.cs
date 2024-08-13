using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using HK.Framework;
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
            var itemTableDatabase = new Dictionary<int, List<MasterData.ItemTable>>();
            var createdItemIds = new HashSet<int>();
            var floorItemNoCosts = CurrentDungeonSpec.FloorItemNoCosts
                .OrderBy(_ => Random.value)
                .Take(Random.Range(CurrentDungeonSpec.NoCostItemNumberMin, CurrentDungeonSpec.NoCostItemNumberMax));
            foreach (var floorItem in floorItemNoCosts)
            {
                var position = new Vector2Int(floorItem.X, floorItem.Y);
                var floorData = DungeonInstanceFloorData.CreateAsItem
                (
                    position,
                    CreateItemList(floorItem.ItemTableId)
                );
                FloorDatabase.Add(position, floorData);
            }
            var floorItemEnemyPlaces = CurrentDungeonSpec.FloorItemEnemyPlaces
                .Where(x => !FloorDatabase.ContainsKey(new Vector2Int(x.X, x.Y)))
                .OrderBy(_ => Random.value)
                .Take(Random.Range(CurrentDungeonSpec.EnemyPlaceItemNumberMin, CurrentDungeonSpec.EnemyPlaceItemNumberMax));
            foreach (var floorItem in floorItemEnemyPlaces)
            {
                var position = new Vector2Int(floorItem.X, floorItem.Y);
                var floorData = DungeonInstanceFloorData.CreateAsItem
                (
                    position,
                    CreateItemList(floorItem.ItemTableId)
                );
                FloorDatabase.Add(position, floorData);

                position = new Vector2Int(floorItem.EnemyPositionX, floorItem.EnemyPositionY);
                if (FloorDatabase.ContainsKey(position))
                {
                    continue;
                }
                var enemyData = DungeonInstanceFloorData.CreateAsEnemy
                (
                    position,
                    masterData.EnemyTables.Get(floorItem.EnemyTableId).Lottery().EnemyId
                );
                FloorDatabase.Add(position, enemyData);
            }
            foreach (var floorItem in CurrentDungeonSpec.SavePoints)
            {
                var position = new Vector2Int(floorItem.X, floorItem.Y);
                var floorData = DungeonInstanceFloorData.CreateAsSavePoint(position);
                FloorDatabase.Add(position, floorData);
            }
            foreach (var wallEvent in CurrentDungeonSpec.FloorItemGuaranteeds)
            {
                var position = new Vector2Int(wallEvent.X, wallEvent.Y);
                Assert.IsFalse(FloorDatabase.ContainsKey(position), $"すでに床データが存在しています position:{position}");
                var floorData = DungeonInstanceFloorData.CreateAsItem
                (
                    position,
                    CreateItemList(wallEvent.ItemTableId)
                );
                FloorDatabase.Add(position, floorData);
            }
            foreach (var floorEnemy in CurrentDungeonSpec.FloorEnemyGuaranteeds)
            {
                var position = new Vector2Int(floorEnemy.X, floorEnemy.Y);
                Assert.IsFalse(FloorDatabase.ContainsKey(position), $"すでに床データが存在しています position:{position}");
                var floorData = DungeonInstanceFloorData.CreateAsEnemy
                (
                    position,
                    masterData.EnemyTables.Get(floorEnemy.EnemyTableId).Lottery().EnemyId
                );
                FloorDatabase.Add(position, floorData);
            }
            WallDatabase.Clear();
            foreach (var wallEvent in CurrentDungeonSpec.WallEvents)
            {
                WallDatabase.Add(
                    wallEvent.GetWallPosition(),
                    new DungeonInstanceWallData(wallEvent)
                );
            }

            List<(MasterData.Item item, int count)> CreateItemList(int itemTableId)
            {
                if (!itemTableDatabase.ContainsKey(itemTableId))
                {
                    itemTableDatabase.Add(itemTableId, new List<MasterData.ItemTable>(masterData.ItemTables.Get(itemTableId)));
                }
                var itemTables = itemTableDatabase[itemTableId];
                int lotteryIndex;
                MasterData.ItemTable itemTable;
                while (true)
                {
                    lotteryIndex = itemTables.LotteryIndex();
                    itemTable = itemTables[lotteryIndex];
                    if (!createdItemIds.Contains(itemTable.ItemId))
                    {
                        break;
                    }
                    else
                    {
                        itemTables.RemoveAt(lotteryIndex);
                    }
                }
                var itemList = new List<(MasterData.Item item, int count)>
                {
                    (itemTable.ItemId.GetMasterDataItem(), itemTable.Count)
                };
                itemTables.RemoveAt(lotteryIndex);
                createdItemIds.Add(itemTable.ItemId);
                return itemList;
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

        private async UniTask InvokeOnItemAsync(Character character, DungeonInstanceFloorData floorData)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            gameEvents.OnAcquiredFloorData.OnNext(floorData);
            FloorDatabase.Remove(character.Position);
            foreach (var (item, count) in floorData.Items)
            {
                character.Inventory.Add(item.Id, count);
                if (count == 1)
                {
                    gameEvents.OnRequestShowMessage.OnNext(new($"<color=#8888FF>{item.Name}</color>を手に入れた。", "Sfx.Message.0"));
                }
                else
                {
                    gameEvents.OnRequestShowMessage.OnNext(new($"<color=#8888FF>{item.Name}</color>を{count}個手に入れた。", "Sfx.Message.0"));
                }
                character.Events.OnAcquiredItem.OnNext((item.Id, count));
                var acquireItemViewScope = new CancellationTokenSource();
                AcquireItemView.OpenAsync(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.AcquireItem"), acquireItemViewScope.Token).Forget();
                await gameEvents.WaitForSubmitInputAsync();
                acquireItemViewScope.Cancel();
                acquireItemViewScope.Dispose();
            }
        }

        private async UniTask InvokeOnSavePointAsync(Character character)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            await gameEvents.ShowMessageAndWaitForSubmitInputAsync(new("ここはセーブポイントのようだ。一休みしよう。", "Sfx.Message.0"));
            character.InstanceStatus.FullRecovery();
            checkPoint = character.Position;
            var view = new GameSavePointMenuView(gameMenuBundlePrefab, character);
            await view.OpenAsync();
        }

        private async UniTask InvokeOnEnemyAsync(Character character, DungeonInstanceFloorData floorData)
        {
            var result = await BeginBattleAsync(character, floorData.EnemyId.GetMasterDataEnemy());
            if (result == Define.BattleResult.PlayerWin)
            {
                TinyServiceLocator.Resolve<GameEvents>().OnAcquiredFloorData.OnNext(floorData);
            }
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
                        TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext(new("扉が開いた", "Sfx.OpenDoor.0"));
                        wallEvent.Open();
                        AddReachedPoint(character);
                        await view.OnOpenDoorAsync(wallEvent);
                    }
                    break;
                case "Lock":
                    if (!wallEvent.IsOpen)
                    {
                        TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext(new("こちらからは開かないようだ", "Sfx.Message.0"));
                    }
                    break;
                case "Item":
                    if (!wallEvent.IsOpen)
                    {
                        foreach (var i in wallEvent.NeedItems)
                        {
                            if (!character.Inventory.HasItem(i))
                            {
                                TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext(new("鍵が必要のようだ", "Sfx.Message.0"));
                                return;
                            }
                        }
                        TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext(new("扉が開いた", "Sfx.OpenDoor.0"));
                        wallEvent.Open();
                        AddReachedPoint(character);
                        await view.OnOpenDoorAsync(wallEvent);
                    }
                    break;
            }
        }

        private async UniTask<Define.BattleResult> BeginBattleAsync(Character character, MasterData.Enemy masterDataEnemy)
        {
            var scope = new CancellationTokenSource();
            var playerCharacter = new BattleCharacter(character, Define.AllyType.Player, new Input(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.Command")));
            var enemyCharacter = masterDataEnemy.CreateBattleCharacter();
            BehaviourPointView.OpenAsync(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.BehaviourPoint"), playerCharacter, scope.Token).Forget();
            GameEnemyView.OpenAsync(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.Enemy"), masterDataEnemy, enemyCharacter, scope.Token).Forget();
            TinyServiceLocator.Resolve<GameEvents>().OnRequestPlayBgm.OnNext($"Bgm.Battle.{masterDataEnemy.BattleBgmId}");
            var battleSystem = new BattleSystem(playerCharacter, enemyCharacter);
            var battleResult = await battleSystem.BeginAsync(scope.Token);
            character.InstanceStatus.ResetGuardPoint();
            TinyServiceLocator.Resolve<GameEvents>().OnRequestPlayBgm.OnNext("Bgm.Exploration.0");
            if (battleResult == Define.BattleResult.PlayerWin)
            {
                character.InstanceStatus.AddExperience(enemyCharacter.BattleStatus.Experience);
                FloorDatabase.Remove(character.Position);
            }
            else
            {
                character.Warp(checkPoint);
                character.InstanceStatus.FullRecovery();
                TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext(new("どうやら安全な場所に移動されたようだ", "Sfx.Message.0"));
            }
            scope.Cancel();
            scope.Dispose();
            return battleResult;
        }

#if DEBUG
        public void DebugAddAllReachedPoint()
        {
            reachedPoints.Clear();
            for (var y = 0; y <= CurrentDungeon.range.y; y++)
            {
                for (var x = 0; x <= CurrentDungeon.range.x; x++)
                {
                    var position = new Vector2Int(x, y);
                    reachedPoints.Add(position);
                    TinyServiceLocator.Resolve<GameEvents>().OnAddReachedPoint.OnNext(position);
                }
            }
        }

        public void DebugBeginBattle(Character player, int masterDataEnemyId)
        {
            var masterData = TinyServiceLocator.Resolve<MasterData>();
            var enemy = masterData.Enemies.Get(masterDataEnemyId);
            BeginBattleAsync(player, enemy).Forget();
        }
#endif
    }
}
