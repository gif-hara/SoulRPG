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
            player.Direction = CurrentDungeonSpec.Direction;
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
                var itemData = CreateItemList(i.ItemTableId);
                if (itemData == default)
                {
                    continue;
                }
                var floorData = new DungeonInstanceFloorData.Item
                (
                    position,
                    itemData.item,
                    itemData.count
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
                var itemData = CreateItemList(i.ItemTableId);
                if (itemData != default)
                {
                    var floorData = new DungeonInstanceFloorData.Item
                    (
                        position,
                        itemData.item,
                        itemData.count
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

            var enemyNoCosts = CurrentDungeonSpec.FloorEnemyNoCosts
                .Where(x => !Enemies.Any(y => y.Position == new Vector2Int(x.X, x.Y)))
                .OrderBy(_ => Random.value)
                .Take(Random.Range(CurrentDungeonSpec.NoCostEnemyNumberMin, CurrentDungeonSpec.NoCostEnemyNumberMax));
            foreach (var i in enemyNoCosts)
            {
                CreateEnemy(masterData.EnemyTables.Get(i.EnemyTableId).Lottery().EnemyId, new Vector2Int(i.X, i.Y));
            }

            foreach (var i in CurrentDungeonSpec.FloorItemGuaranteeds)
            {
                var position = new Vector2Int(i.X, i.Y);
                var itemData = CreateItemList(i.ItemTableId);
                if (itemData == default)
                {
                    continue;
                }
                var floorData = new DungeonInstanceFloorData.Item
                (
                    position,
                    itemData.item,
                    itemData.count
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
                    i.SequenceId.GetFloorEventSequenceData().Sequences,
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

            (MasterData.Item item, int count) CreateItemList(int itemTableId)
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
                        return default;
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

                itemTables.RemoveAt(lotteryIndex);
                createdItemIds.Add(itemTable.ItemId);
                return (itemTable.ItemId.GetMasterDataItem(), itemTable.Count);
            }

            void CreateEnemy(int masterDataEnemyId, Vector2Int position)
            {
                var enemy = new Character(masterData.Enemies.Get(masterDataEnemyId));
                Enemies.Add(enemy);
                enemy.Warp(position);
                EnemyController.Attach(enemy, player, this, false);
            }
        }

        public void NextFloor(int floorId)
        {
            currentFloorId = floorId;
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
                    DungeonInstanceFloorData.SequenceEvent sequenceEventData => OnInteractSequenceEventAsync(character, sequenceEventData),
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

        public bool IsExistEnemy(Vector2Int position)
        {
            return Enemies.Any(x => x.Position == position);
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
            character.Inventory.Add(itemData.MasterDataItem.Id, itemData.Count);
            if (itemData.Count == 1)
            {
                var message = "<color=#8888FF>{0}</color>を手に入れた。".Localized().Format(itemData.MasterDataItem.Name);
                gameEvents.OnRequestShowMessage.OnNext(new(message, "Sfx.Message.0"));
            }
            else
            {
                var message = "<color=#8888FF>{0}</color>を{1}個手に入れた。".Localized().Format(itemData.MasterDataItem.Name, itemData.Count);
                gameEvents.OnRequestShowMessage.OnNext(new(message, "Sfx.Message.0"));
            }

            character.Events.OnAcquiredItem.OnNext((itemData.MasterDataItem.Id, itemData.Count));
            var acquireItemViewScope = new CancellationTokenSource();
            AcquireItemView.OpenAsync(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.AcquireItem"), itemData.MasterDataItem,
                acquireItemViewScope.Token).Forget();
            await gameEvents.WaitForSubmitInputAsync();
            acquireItemViewScope.Cancel();
            acquireItemViewScope.Dispose();
        }

        private async UniTask OnInteractSequenceEventAsync(Character character,
            DungeonInstanceFloorData.SequenceEvent sequenceData)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            gameEvents.OnRequestChangeMiniMapType.OnNext(Define.MiniMapType.Default);
            var inputController = TinyServiceLocator.Resolve<InputController>();
            inputController.PushInputType(InputController.InputType.UI);
            var container = new Container();
            container.Register<DungeonInstanceFloorData>("CurrentEvent", sequenceData);
            container.Register("CurrentViewDocument", view.GetFloorEventDocument(sequenceData));
            await new Sequencer(container, sequenceData.Sequences.Sequences).PlayAsync(scope.Token);
            inputController.PopInputType();
            EnterAsync(character).Forget();
        }

        private async UniTask OnEnterEnemyAsync(Character player, Character enemy)
        {
#if DEBUG
            if (TinyServiceLocator.Resolve<BattleDebugData>().NoEncount)
            {
                return;
            }
#endif
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
                        gameEvents.OnRequestShowMessage.OnNext(new("扉が開いた。", "Sfx.OpenDoor.0"));
                        wallEvent.Open();
                        AddReachedPoint(character);
                        gameEvents.OnOpenDoor.OnNext(Unit.Default);
                        await view.OnOpenDoorAsync(wallEvent);
                    }

                    break;
                case "Lock":
                    if (!wallEvent.IsOpen)
                    {
                        gameEvents.OnRequestShowMessage.OnNext(new("こちらからは開かないようだ。", "Sfx.Message.0"));
                    }

                    break;
            }
        }

        public async UniTask<Define.BattleResult> BeginBattleAsync(Character character,
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
            var battleInformationEnemyView = new BattleInformationEnemyView();
            battleInformationEnemyView.OpenAsync(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.Battle.Info.Enemy"), enemyCharacter, scope.Token).Forget();
            TinyServiceLocator.Register(battleInformationEnemyView);
            var gameEnemyView = new GameEnemyView(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.Enemy"), scope.Token);
            gameEnemyView.Open(masterDataEnemy, enemyCharacter, scope.Token);
            TinyServiceLocator.Resolve<GameEvents>().OnRequestPlayBgm
                .OnNext($"Bgm.Battle.{masterDataEnemy.BattleBgmId}");
            var battleSystem = new BattleSystem(playerCharacter, enemyCharacter);
            var battleResult = await battleSystem.BeginAsync(scope.Token);
            character.InstanceStatus.ResetGuardPoint();
            if (battleResult == Define.BattleResult.PlayerWin)
            {
                if (masterDataEnemy.PlayBgmIfDefeated)
                {
                    TinyServiceLocator.Resolve<GameEvents>().OnRequestPlayBgm.OnNext("Bgm.Exploration.0");
                }
                else
                {
                    AudioManager.StopBgm();
                }
                var addExperience = await playerCharacter.AilmentController.OnCalculateAddExperienceAsync(enemyCharacter.BattleStatus.Experience);
                character.InstanceStatus.AddExperience(addExperience);
                await gameEvents.ShowMessageAndWaitForSubmitInputAsync(new($"<color=#88FF88>{addExperience}</color>の経験値を獲得した。", "Sfx.AcquireExperience.0"));
            }
            else
            {
                TinyServiceLocator.Resolve<GameEvents>().OnRequestPlayBgm.OnNext("Bgm.Exploration.0");
                character.ResetAll();
                Setup(homeDungeonName, character);
                DisposeScope();
                var recoveryScope = new CancellationTokenSource();
                await new Sequencer(new Container(), gameRule.SequenceDatabase.Get("Player.Recovery").Sequences).PlayAsync(recoveryScope.Token);
                recoveryScope.Cancel();
                recoveryScope.Dispose();
            }

            DisposeScope();
            return battleResult;

            void DisposeScope()
            {
                playerCharacter.Dispose();
                enemyCharacter.Dispose();

                if (TinyServiceLocator.Contains<BattleInformationEnemyView>())
                {
                    TinyServiceLocator.Remove<BattleInformationEnemyView>();
                }

                if (scope == null)
                {
                    return;
                }
                scope.Cancel();
                scope.Dispose();
                scope = null;
            }
        }

        public void RemoveFloorData(Vector2Int position)
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
        }

        public bool CanRestCheckPoint(Vector2Int position)
        {
            return !restedCheckPoints.Contains(position);
        }

        public UniTask<Define.BattleResult> BeginBattleAsync(Character player, int masterDataEnemyId)
        {
            var masterData = TinyServiceLocator.Resolve<MasterData>();
            var enemy = masterData.Enemies.Get(masterDataEnemyId);
            return BeginBattleAsync(player, enemy);
        }

        public SuspendData.DungeonData CreateSuspendData()
        {
            return new SuspendData.DungeonData
            {
                dungeonName = CurrentDungeon.name,
                floorId = currentFloorId,
                itemData = FloorDatabase.Values
                    .OfType<DungeonInstanceFloorData.Item>()
                    .Select(x => new SuspendData.DungeonInstanceItemData
                    {
                        position = x.Position,
                        itemId = x.MasterDataItem.Id,
                        count = x.Count
                    })
                    .ToArray(),
                sequenceEventData = FloorDatabase.Values
                    .OfType<DungeonInstanceFloorData.SequenceEvent>()
                    .Select(x => new SuspendData.DungeonInstanceSequenceEventData
                    {
                        position = x.Position,
                        viewName = x.ViewName,
                        promptMessage = x.PromptMessage,
                        sequenceId = x.Sequences.name
                    })
                    .ToArray(),
                enemyData = Enemies
                    .Select(x => new SuspendData.DungeonEnemyData
                    {
                        position = x.Position,
                        enemyId = x.MasterDataEnemy.Id,
                        findPlayer = x.FindPlayer
                    })
                    .ToArray(),
                wallData = WallDatabase.Values
                    .Select(x => new SuspendData.DungeonInstanceWallData
                    {
                        from = x.From,
                        to = x.To,
                        isOpen = x.IsOpen,
                        eventType = x.EventType,
                        positiveSideCondition = x.PositiveSideCondition,
                        negativeSideCondition = x.NegativeSideCondition
                    })
                    .ToArray(),
                reachedPositionData = reachedPoints
                    .Select(x => new SuspendData.ReachedPositionData
                    {
                        position = x
                    })
                    .ToArray()
            };
        }

        public void SyncFromSuspendData(SuspendData.DungeonData suspendData)
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
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            var masterData = TinyServiceLocator.Resolve<MasterData>();
            CurrentDungeon = masterData.Dungeons.Get(suspendData.dungeonName);
            CurrentDungeonSpec = masterData.DungeonSpecs.Get(CurrentDungeon.name);
            currentFloorId = suspendData.floorId;

            foreach (var i in suspendData.itemData)
            {
                var itemData = new DungeonInstanceFloorData.Item
                (
                    i.position,
                    masterData.Items.Get(i.itemId),
                    i.count
                );
                FloorDatabase.Add(i.position, itemData);
            }

            foreach (var i in suspendData.sequenceEventData)
            {
                var sequenceData = new DungeonInstanceFloorData.SequenceEvent
                (
                    i.position,
                    i.viewName,
                    i.sequenceId.GetFloorEventSequenceData().Sequences,
                    i.promptMessage
                );
                FloorDatabase.Add(i.position, sequenceData);
            }

            foreach (var i in suspendData.enemyData)
            {
                var enemy = new Character(masterData.Enemies.Get(i.enemyId));
                Enemies.Add(enemy);
                enemy.Warp(i.position);
                EnemyController.Attach(enemy, TinyServiceLocator.Resolve<Character>("Player"), this, i.findPlayer);
            }

            foreach (var i in suspendData.wallData)
            {
                var wallPosition = new WallPosition(i.from, i.to);
                var data = new DungeonInstanceWallData(i.from, i.to, i.eventType, i.positiveSideCondition, i.negativeSideCondition, i.isOpen);
                WallDatabase.Add(wallPosition, data);
            }

            foreach (var i in suspendData.reachedPositionData)
            {
                reachedPoints.Add(i.position);
            }

            gameEvents.OnSetupDungeon.OnNext(this);
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
