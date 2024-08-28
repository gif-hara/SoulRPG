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
using UnitySequencerSystem;

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

        public readonly List<Character> Enemies = new();

        private readonly DungeonPathFinder pathFinder = new();

        private readonly CancellationTokenSource scope;

        private CancellationTokenSource enterScope;

        private CancellationTokenSource dungeonScope;
        public CancellationToken DungeonScope => dungeonScope.Token;

        private readonly HashSet<Vector2Int> restedCheckPoints = new();

        private int currentFloorId = 0;

        private readonly string homeDungeonName;

        public DungeonController(
            HKUIDocument gameMenuBundlePrefab,
            IExplorationView view,
            string homeDungeonName,
            CancellationToken scope
        )
        {
            this.gameMenuBundlePrefab = gameMenuBundlePrefab;
            this.view = view;
            this.homeDungeonName = homeDungeonName;
            this.scope = CancellationTokenSource.CreateLinkedTokenSource(scope);
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            gameEvents.OnRequestChangeDungeon
                .Subscribe(x =>
                    {
                        Setup(x, TinyServiceLocator.Resolve<Character>("Player"));
                    })
                .RegisterTo(this.scope.Token);
            gameEvents.OnRequestNextFloor
                .Subscribe(x =>
                    {
                        NextFloor(x);
                    })
                .RegisterTo(this.scope.Token);
        }

        public void Setup(string dungeonName, Character player)
        {
            dungeonScope?.Cancel();
            dungeonScope?.Dispose();
            dungeonScope = new CancellationTokenSource();
            reachedPoints.Clear();
            foreach (var enemy in Enemies)
            {
                enemy.Dispose();
            }
            Enemies.Clear();
            restedCheckPoints.Clear();
            var masterData = TinyServiceLocator.Resolve<MasterData>();
            CurrentDungeon = masterData.Dungeons.Get(dungeonName);
            CurrentDungeonSpec = masterData.DungeonSpecs.Get(CurrentDungeon.name);
            var initialPosition = new Vector2Int(CurrentDungeonSpec.InitialX, CurrentDungeonSpec.InitialY);
            player.Warp(initialPosition);
            checkPoint = initialPosition;
            FloorDatabase.Clear();
            var itemTableDatabase = new Dictionary<int, List<MasterData.ItemTable>>();
            var createdItemIds = new HashSet<int>();
            foreach (var i in player.Inventory.Items)
            {
                createdItemIds.Add(i.Key);
            }
            var floorItemNoCosts = CurrentDungeonSpec.FloorItemNoCosts
                .OrderBy(_ => Random.value)
                .Take(Random.Range(CurrentDungeonSpec.NoCostItemNumberMin, CurrentDungeonSpec.NoCostItemNumberMax));
            foreach (var i in floorItemNoCosts)
            {
                var position = new Vector2Int(i.X, i.Y);
                var items = CreateItemList(i.ItemTableId);
                if (items == null)
                {
                    continue;
                }
                var floorData = new DungeonInstanceFloorData.Item
                (
                    position,
                    items
                );
                FloorDatabase.Add(position, floorData);
            }

            var floorItemEnemyPlaces = CurrentDungeonSpec.FloorItemEnemyPlaces
                .Where(x => !FloorDatabase.ContainsKey(new Vector2Int(x.X, x.Y)))
                .OrderBy(_ => Random.value)
                .Take(Random.Range(CurrentDungeonSpec.EnemyPlaceItemNumberMin,
                    CurrentDungeonSpec.EnemyPlaceItemNumberMax));
            foreach (var i in floorItemEnemyPlaces)
            {
                var position = new Vector2Int(i.X, i.Y);
                var items = CreateItemList(i.ItemTableId);
                if (items != null)
                {
                    var floorData = new DungeonInstanceFloorData.Item
                    (
                        position,
                        items
                    );
                    FloorDatabase.Add(position, floorData);
                }

                position = new Vector2Int(i.EnemyPositionX, i.EnemyPositionY);
                if (FloorDatabase.ContainsKey(position))
                {
                    continue;
                }

                if (Enemies.Any(x => x.Position == position))
                {
                    continue;
                }

                CreateEnemy(masterData.EnemyTables.Get(i.EnemyTableId).Lottery().EnemyId, position);
            }

            foreach (var i in CurrentDungeonSpec.FloorItemGuaranteeds)
            {
                var position = new Vector2Int(i.X, i.Y);
                var items = CreateItemList(i.ItemTableId);
                if (items == null)
                {
                    continue;
                }
                var floorData = new DungeonInstanceFloorData.Item
                (
                    position,
                    items
                );
                AddFloorData(position, floorData);
            }

            foreach (var i in CurrentDungeonSpec.FloorEnemyGuaranteeds)
            {
                CreateEnemy(masterData.EnemyTables.Get(i.EnemyTableId).Lottery().EnemyId, new Vector2Int(i.X, i.Y));
            }

            foreach (var i in CurrentDungeonSpec.FloorEvents)
            {
                var position = new Vector2Int(i.X, i.Y);
                var floorData = new DungeonInstanceFloorData.SequenceEvent
                (
                    position,
                    i.ViewName,
                    i.Sequences,
                    i.PromptMessage
                );
                AddFloorData(position, floorData);
            }

            WallDatabase.Clear();
            foreach (var wallEvent in CurrentDungeonSpec.WallEvents)
            {
                WallDatabase.Add(
                    wallEvent.GetWallPosition(),
                    new DungeonInstanceWallData(wallEvent)
                );
            }
            AddReachedPoint(player);
            TinyServiceLocator.Resolve<GameEvents>().OnSetupDungeon.OnNext(this);

            void AddFloorData(Vector2Int position, DungeonInstanceFloorData floorData)
            {
                Assert.IsFalse(FloorDatabase.ContainsKey(position), $"すでに床データが存在しています position:{position}");
                FloorDatabase.Add(position, floorData);
            }

            List<(MasterData.Item item, int count)> CreateItemList(int itemTableId)
            {
                if (!itemTableDatabase.ContainsKey(itemTableId))
                {
                    itemTableDatabase.Add(itemTableId,
                        new List<MasterData.ItemTable>(masterData.ItemTables.Get(itemTableId)));
                }

                var itemTables = itemTableDatabase[itemTableId];
                int lotteryIndex;
                MasterData.ItemTable itemTable;
                while (true)
                {
                    if (itemTables.Count == 0)
                    {
                        return null;
                    }
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

            void CreateEnemy(int masterDataEnemyId, Vector2Int position)
            {
                var enemy = new Character(masterData.Enemies.Get(masterDataEnemyId));
                Enemies.Add(enemy);
                enemy.Warp(position);
                EnemyController.Attach(enemy, player, this);
            }
        }

        public void NextFloor(int floorId)
        {
            var masterData = TinyServiceLocator.Resolve<MasterData>();
            var dungeonTables = masterData.DungeonTables.Get(floorId);
            Assert.IsNotNull(dungeonTables, $"ダンジョンが存在しません id:{floorId}");
            var dungeonTable = dungeonTables[Random.Range(0, dungeonTables.Count)];
            Setup(dungeonTable.DungeonName, TinyServiceLocator.Resolve<Character>("Player"));
        }

        public void NextFloor()
        {
            currentFloorId++;
            NextFloor(currentFloorId);
        }

        public UniTask EnterAsync(Character character)
        {
            enterScope?.Cancel();
            enterScope?.Dispose();
            enterScope = new CancellationTokenSource();
            AddReachedPoint(character);
            var enemy = Enemies.Find(x => x.Position == character.Position);
            if (enemy != null)
            {
                return OnEnterEnemyAsync(character, enemy);
            }

            if (FloorDatabase.TryGetValue(character.Position, out var floorData))
            {
                return floorData switch
                {
                    DungeonInstanceFloorData.Item itemData => OnEnterItemAsync(itemData),
                    DungeonInstanceFloorData.SequenceEvent sequenceData => OnEnterSequenceEventAsync(sequenceData),
                    _ => UniTask.CompletedTask,
                };
            }
            else if (WallDatabase.TryGetValue(character.Direction.GetWallPosition(character.Position),
                         out var wallEvent))
            {
                return OnEnterDoorAsync(wallEvent);
            }
            else
            {
                return UniTask.CompletedTask;
            }
        }

        public UniTask InteractAsync(Character character)
        {
            enterScope?.Cancel();
            enterScope?.Dispose();
            enterScope = null;
            var wallPosition = character.Direction.GetWallPosition(character.Position);
            if (FloorDatabase.TryGetValue(character.Position, out var floorData))
            {
                return floorData switch
                {
                    DungeonInstanceFloorData.Item itemData => OnInteractItemAsync(character, itemData),
                    DungeonInstanceFloorData.SequenceEvent messageData => OnInteractSequenceEventAsync(character,
                        messageData),
                    _ => UniTask.CompletedTask,
                };
            }
            else if (WallDatabase.TryGetValue(wallPosition, out var wallEvent))
            {
                return wallEvent.EventType switch
                {
                    "Door" => OnInteractDoorAsync(character, wallEvent),
                    _ => UniTask.CompletedTask,
                };
            }
            else
            {
                return UniTask.CompletedTask;
            }
        }

        public void BeginForceBattle(Character player, Character enemy)
        {
            OnEnterEnemyAsync(player, enemy).Forget();
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

        private async UniTask OnInteractItemAsync(Character character, DungeonInstanceFloorData.Item itemData)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            gameEvents.OnRequestChangeMiniMapType.OnNext(Define.MiniMapType.Default);
            gameEvents.OnAcquiredFloorData.OnNext(itemData);
            RemoveFloorData(character.Position);
            foreach (var (item, count) in itemData.Items)
            {
                character.Inventory.Add(item.Id, count);
                if (count == 1)
                {
                    gameEvents.OnRequestShowMessage.OnNext(new($"<color=#8888FF>{item.Name}</color>を手に入れた。",
                        "Sfx.Message.0"));
                }
                else
                {
                    gameEvents.OnRequestShowMessage.OnNext(new($"<color=#8888FF>{item.Name}</color>を{count}個手に入れた。",
                        "Sfx.Message.0"));
                }

                character.Events.OnAcquiredItem.OnNext((item.Id, count));
                var acquireItemViewScope = new CancellationTokenSource();
                AcquireItemView.OpenAsync(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.AcquireItem"), item,
                    acquireItemViewScope.Token).Forget();
                await gameEvents.WaitForSubmitInputAsync();
                acquireItemViewScope.Cancel();
                acquireItemViewScope.Dispose();
            }
        }

        private async UniTask OnInteractSequenceEventAsync(Character character,
            DungeonInstanceFloorData.SequenceEvent sequenceData)
        {
            var inputController = TinyServiceLocator.Resolve<InputController>();
            inputController.PushInputType(InputController.InputType.UI);
            var container = new Container();
            await new Sequencer(container, sequenceData.Sequences.Sequences).PlayAsync(scope.Token);
            inputController.PopInputType();
            EnterAsync(character).Forget();
        }

        private async UniTask OnEnterEnemyAsync(Character player, Character enemy)
        {
            var result = await BeginBattleAsync(player, enemy.MasterDataEnemy);
            if (result == Define.BattleResult.PlayerWin)
            {
                Enemies.Remove(enemy);
                enemy.Dispose();
            }
        }

        private UniTask OnEnterItemAsync(DungeonInstanceFloorData.Item itemData)
        {
            var scope = CancellationTokenSource.CreateLinkedTokenSource(enterScope.Token, itemData.LifeScope.Token);
            var inputController = TinyServiceLocator.Resolve<InputController>();
            TinyServiceLocator.Resolve<GameEvents>().OnRequestShowInputGuideCenter.OnNext(
                (() => $"{inputController.InputActions.InGame.Interact.GetTag()}アイテムを拾う", scope.Token));
            return UniTask.CompletedTask;
        }

        private UniTask OnEnterSequenceEventAsync(DungeonInstanceFloorData.SequenceEvent sequenceData)
        {
            var scope = CancellationTokenSource.CreateLinkedTokenSource(enterScope.Token, sequenceData.LifeScope.Token);
            var inputController = TinyServiceLocator.Resolve<InputController>();
            TinyServiceLocator.Resolve<GameEvents>().OnRequestShowInputGuideCenter.OnNext(
                (() => $"{inputController.InputActions.InGame.Interact.GetTag()}{sequenceData.PromptMessage}",
                    scope.Token));
            return UniTask.CompletedTask;
        }

        private UniTask OnEnterDoorAsync(DungeonInstanceWallData wallEvent)
        {
            if (wallEvent.IsOpen)
            {
                return UniTask.CompletedTask;
            }

            var scope = CancellationTokenSource.CreateLinkedTokenSource(enterScope.Token);
            var inputController = TinyServiceLocator.Resolve<InputController>();
            TinyServiceLocator.Resolve<GameEvents>().OnRequestShowInputGuideCenter.OnNext(
                (() => $"{inputController.InputActions.InGame.Interact.GetTag()}開ける", scope.Token));
            return UniTask.CompletedTask;
        }

        private async UniTask OnInteractDoorAsync(Character character, DungeonInstanceWallData wallEvent)
        {
            var isPositiveAccess = wallEvent.IsPositiveAccess(character.Direction);
            var condition = isPositiveAccess ? wallEvent.PositiveSideCondition : wallEvent.NegativeSideCondition;
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            switch (condition)
            {
                case "None":
                    if (!wallEvent.IsOpen)
                    {
                        gameEvents.OnRequestShowMessage.OnNext(new("扉が開いた", "Sfx.OpenDoor.0"));
                        wallEvent.Open();
                        AddReachedPoint(character);
                        gameEvents.OnOpenDoor.OnNext(Unit.Default);
                        await view.OnOpenDoorAsync(wallEvent);
                    }

                    break;
                case "Lock":
                    if (!wallEvent.IsOpen)
                    {
                        gameEvents.OnRequestShowMessage.OnNext(new("こちらからは開かないようだ", "Sfx.Message.0"));
                    }

                    break;
                case "Item":
                    if (!wallEvent.IsOpen)
                    {
                        foreach (var i in wallEvent.NeedItems)
                        {
                            if (!character.Inventory.HasItem(i))
                            {
                                gameEvents.OnRequestShowMessage.OnNext(new("鍵が必要のようだ", "Sfx.Message.0"));
                                return;
                            }
                        }

                        gameEvents.OnRequestShowMessage.OnNext(new("扉が開いた", "Sfx.OpenDoor.0"));
                        wallEvent.Open();
                        AddReachedPoint(character);
                        await view.OnOpenDoorAsync(wallEvent);
                    }

                    break;
            }
        }

        private async UniTask<Define.BattleResult> BeginBattleAsync(Character character,
            MasterData.Enemy masterDataEnemy)
        {
            var scope = new CancellationTokenSource();
            TinyServiceLocator.Resolve<InputController>().PushInputType(InputController.InputType.UI, scope.Token);
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            gameEvents.OnRequestChangeMiniMapType.OnNext(Define.MiniMapType.Default);
            var gameRule = TinyServiceLocator.Resolve<GameRule>();
            var playerCharacter = new BattleCharacter(
                character,
                Define.AllyType.Player,
                new Input(
                    gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.Command"),
                    gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.List"),
                    gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Info.Ailment"),
                    gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.Battle.Info.Weapon"),
                    gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Info.Status"),
                    gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Info.Item")
                ),
                gameRule.PlayerBattleCharacterSequences
            );
            var enemyCharacter = masterDataEnemy.CreateBattleCharacter();
            var sequences = gameRule.SequenceDatabase.Get("Battle.Begin.0");
            var container = new Container();
            await new Sequencer(container, sequences.Sequences).PlayAsync(scope.Token);
            BehaviourPointView.OpenAsync(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.BehaviourPoint"),
                playerCharacter, scope.Token).Forget();
            MagicCountView.OpenAsync(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.MagicCount"), playerCharacter,
                scope.Token).Forget();
            KnifeCountView.OpenAsync(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.KnifeCount"), playerCharacter,
                scope.Token).Forget();
            TinyServiceLocator.Resolve<ScreenEffectView>().Subscribe(playerCharacter, scope.Token);
            TinyServiceLocator.Resolve<ExplorationView>().BeginSubscribe(playerCharacter, scope.Token);
            var gameEnemyView = new GameEnemyView(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.Enemy"), scope.Token);
            gameEnemyView.Open(masterDataEnemy, enemyCharacter, scope.Token);
            TinyServiceLocator.Resolve<GameEvents>().OnRequestPlayBgm
                .OnNext($"Bgm.Battle.{masterDataEnemy.BattleBgmId}");
            var battleSystem = new BattleSystem(playerCharacter, enemyCharacter);
            var battleResult = await battleSystem.BeginAsync(scope.Token);
            character.InstanceStatus.ResetGuardPoint();
            TinyServiceLocator.Resolve<GameEvents>().OnRequestPlayBgm.OnNext("Bgm.Exploration.0");
            if (battleResult == Define.BattleResult.PlayerWin)
            {
                character.InstanceStatus.AddExperience(enemyCharacter.BattleStatus.Experience);
            }
            else
            {
                character.InstanceStatus.FullRecovery();
                Setup(homeDungeonName, character);
                TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage
                    .OnNext(new("どうやら安全な場所に移動されたようだ", "Sfx.Message.0"));
            }

            scope.Cancel();
            scope.Dispose();
            return battleResult;
        }

        private void RemoveFloorData(Vector2Int position)
        {
            if (FloorDatabase.TryGetValue(position, out var floorData))
            {
                floorData.OnRemove();
                FloorDatabase.Remove(position);
            }
        }

        public Vector2Int? FindNextPosition(Vector2Int start, Vector2Int goal)
        {
            return pathFinder.FindPath(this, start, goal);
        }

        public void RestCheckPoint(Vector2Int position)
        {
            restedCheckPoints.Add(position);
            checkPoint = position;
        }

        public bool CanRestCheckPoint(Vector2Int position)
        {
            return !restedCheckPoints.Contains(position);
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
