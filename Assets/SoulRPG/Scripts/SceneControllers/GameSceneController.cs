using System;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.CharacterControllers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace SoulRPG.SceneControllers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameSceneController : MonoBehaviour
    {
        [SerializeField]
        private MasterData masterData;

        [SerializeField]
        private GameRule gameRule;

        [SerializeField]
        private GameCameraController gameCameraControllerPrefab;

        [SerializeField]
        private HKUIDocument gameUIDocumentPrefab;

        [SerializeField]
        private HKUIDocument gameMenuBundlePrefab;

        [SerializeField]
        private HKUIDocument dungeonDocumentPrefab;

        [SerializeField]
        private HKUIDocument commandDocumentPrefab;

        [SerializeField]
        private string homeDungeonName;

        [SerializeField]
        private string debugDungeonName;

        [SerializeField]
        private Vector2Int debugPosition;

        [SerializeField]
        private bool isUseDebugPosition;

        [SerializeField]
        private CharacterGrowthParameter debugPlayerGrowthParameter;

        [SerializeField]
        private bool isUseDebugGrowthParameter;

        [SerializeField]
        private string debugPlayerName;

        [SerializeField]
        private bool isDebugIgnoreNameInput;

        [SerializeField]
        private Define.CharacterAttribute debugPlayerAttribute;

        [SerializeField]
        private int debugEnemyMasterDataId;

        [SerializeField]
        private CharacterBattleStatusBlueprint debugEnemyBattleStatus;

        [SerializeField]
        private bool isDebugIgnoreTitleScreen;

        async void Start()
        {
            try
            {
                await BootSystem.IsReady;
                TinyServiceLocator.Register(masterData);
                var gameEvents = new GameEvents();
                TinyServiceLocator.Register(gameEvents);
                TinyServiceLocator.Register(gameRule);
#if DEBUG
                GameDebugInformationView.OpenAsync(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.DebugPanel"), destroyCancellationToken).Forget();
#endif
                TinyServiceLocator.Register(new GameFadeView(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.Fade"), destroyCancellationToken));
                var saveData = SaveData.LoadSafe();
                Assert.IsNotNull(saveData, "saveData != null");
                Assert.IsNotNull(saveData.playerData, "saveData.playerData != null");
                var suspendData = SuspendData.Load();
                var playerName = saveData?.playerData?.name ?? debugPlayerName;
                var playerGrowthParameter = suspendData?.growthParameter ?? new(gameRule.PlayerGrowthParameter);
                var player = new Character(playerName, playerGrowthParameter, gameRule.InitialEquipment, debugPlayerAttribute);
                TinyServiceLocator.Register("Player", player);
                var gameCameraController = Instantiate(gameCameraControllerPrefab);
                gameCameraController.Setup(player);
                var explorationView = new ExplorationView(
                    gameUIDocumentPrefab,
                    dungeonDocumentPrefab,
                    gameCameraController,
                    player
                    );
                TinyServiceLocator.Register(explorationView);
                var playerController = new PlayerController();
                TinyServiceLocator.Register(new InputScheme(destroyCancellationToken));
                var inputController = new InputController();
                TinyServiceLocator.Register(inputController);
                var dungeonController = new DungeonController(
                    gameMenuBundlePrefab,
                    explorationView,
                    homeDungeonName,
                    destroyCancellationToken
                    );
                TinyServiceLocator.Register(dungeonController);
                explorationView.Open(destroyCancellationToken);
#if DEBUG
                if (isUseDebugPosition)
                {
                    player.Warp(debugPosition);
                }
                if (isUseDebugGrowthParameter)
                {
                    player.GrowthParameter.Sync(debugPlayerGrowthParameter, player);
                    player.InstanceStatus.FullRecovery();
                }
                if (string.IsNullOrEmpty(saveData.playerData.name) && isDebugIgnoreNameInput)
                {
                    saveData.playerData.name = debugPlayerName;
                    saveData.Save();
                }
#endif
                foreach (var i in gameRule.InitialItemDatabase)
                {
                    player.Inventory.Add(i.ItemId, i.Count);
                }
                playerController.Attach(player, gameMenuBundlePrefab, destroyCancellationToken);
                gameEvents.OnRequestShowMessage.Select(x => x.SfxName)
                .Subscribe(x =>
                    {
                        if (string.IsNullOrEmpty(x))
                        {
                            return;
                        }
                        AudioManager.PlaySfx(gameRule.AudioDatabase.Get(x).Clip);
                    })
                    .RegisterTo(destroyCancellationToken);
                gameEvents.OnRequestPlayBgm
                    .Subscribe(x =>
                    {
                        AudioManager.PlayBgm(gameRule.AudioDatabase.Get(x).Clip);
                    })
                    .RegisterTo(destroyCancellationToken);
                var inputGuideCenter = new GameInputGuideView(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.InputGuide.Center"), destroyCancellationToken);
                gameEvents.OnRequestShowInputGuideCenter
                    .Subscribe(x =>
                    {
                        inputGuideCenter.Open(x.messageSelector, x.scope);
                    })
                    .RegisterTo(destroyCancellationToken);
                var inputGuideBottom = new GameInputGuideView(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.InputGuide.Bottom"), destroyCancellationToken);
                gameEvents.OnRequestShowInputGuideBottom
                    .Subscribe(x =>
                    {
                        inputGuideBottom.Open(x.messageSelector, x.scope);
                    })
                    .RegisterTo(destroyCancellationToken);
                var screenEffectView = new ScreenEffectView(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.ScreenEffect"), destroyCancellationToken);
                TinyServiceLocator.Register(screenEffectView);
                inputController.PushInputType(InputController.InputType.InGame);
                var gameTipsView = new GameTipsView(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.Tips"), destroyCancellationToken);
                TinyServiceLocator.Register(gameTipsView);

#if !DEBUG
                isDebugIgnoreTitleScreen = false;
#endif
                if (!isDebugIgnoreTitleScreen)
                {
                    await GameTitleScreenView.OpenAsync(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.TitleScreen"), destroyCancellationToken);
                    gameEvents.OnRequestPlayBgm.OnNext("Bgm.Exploration.0");
                    GameFadeView.BeginFadeAsync(new Color(0.0f, 0.0f, 0.0f, 0.0f), 1.0f, destroyCancellationToken).Forget();
                }
                else
                {
                    gameEvents.OnRequestPlayBgm.OnNext("Bgm.Exploration.0");
                }

                if (suspendData != null)
                {
                    player.SyncFromSuspendData(suspendData);
                    dungeonController.SyncFromSuspendData(suspendData.dungeonData);
                    SuspendData.Delete();
                    inputController.PushInputType(InputController.InputType.UI);
                    await DialogView.ConfirmAsync(
                        gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Dialog"),
                        "中断データから再開しました。中断データは削除されたのでご注意ください。",
                        new[] { "確認" },
                        0,
                        destroyCancellationToken
                        );
                    AudioManager.PlaySfx("Sfx.Message.0");
                    inputController.PopInputType();
                }
                else
                {
                    gameEvents.OnRequestChangeDungeon.OnNext(debugDungeonName);
                }

                if (string.IsNullOrEmpty(saveData.playerData.name))
                {
                    inputController.PushInputType(InputController.InputType.UI);
                    var newPlayerName = await GameNameInputFieldView.OpenAsync(
                        gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.NameInputField"),
                        gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Dialog"),
                        destroyCancellationToken
                    );
                    inputController.PopInputType();
                    saveData.playerData.name = newPlayerName;
                    saveData.Save();
                    player.Name = newPlayerName;
                }
#if DEBUG
                var battleDebugData = new BattleDebugData();
                TinyServiceLocator.Register(battleDebugData);
                Observable.EveryUpdate(destroyCancellationToken)
                    .Subscribe(_ =>
                    {
                        try
                        {
                            if (Keyboard.current.qKey.wasPressedThisFrame)
                            {
                                foreach (var i in TinyServiceLocator.Resolve<MasterData>().Items.List)
                                {
                                    player.Inventory.Add(i.Id, 1);
                                }
                                TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext(new("[DEBUG] Add All Items", "Sfx.Message.0"));
                            }
                            if (Keyboard.current.wKey.wasPressedThisFrame)
                            {
                                dungeonController.DebugBeginBattle(player, debugEnemyMasterDataId);
                            }
                            if (Keyboard.current.eKey.wasPressedThisFrame)
                            {
                                player.InstanceStatus.AddExperience(gameRule.DebugAddExperience);
                                TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext(new($"[DEBUG] Add Experience {gameRule.DebugAddExperience}", "Sfx.Message.0"));
                            }
                            if (Keyboard.current.rKey.wasPressedThisFrame)
                            {
                                dungeonController.DebugAddAllReachedPoint();
                            }
                            if (Keyboard.current.tKey.wasPressedThisFrame)
                            {
                                battleDebugData.IsInvinciblePlayer = !battleDebugData.IsInvinciblePlayer;
                                gameEvents.OnRequestShowMessage.OnNext(new($"プレイヤーの無敵：{battleDebugData.IsInvinciblePlayer}"));
                            }
                            if (Keyboard.current.yKey.wasPressedThisFrame)
                            {
                                battleDebugData.IsInvincibleEnemy = !battleDebugData.IsInvincibleEnemy;
                                gameEvents.OnRequestShowMessage.OnNext(new($"敵の無敵：{battleDebugData.IsInvincibleEnemy}"));
                            }
                            if (Keyboard.current.uKey.wasPressedThisFrame)
                            {
                                battleDebugData.IsAllSkillAvailable = !battleDebugData.IsAllSkillAvailable;
                                gameEvents.OnRequestShowMessage.OnNext(new($"全スキル使用可能：{battleDebugData.IsAllSkillAvailable}"));
                            }
                            if (Keyboard.current.iKey.wasPressedThisFrame)
                            {
                                battleDebugData.NoCost = !battleDebugData.NoCost;
                                gameEvents.OnRequestShowMessage.OnNext(new($"コスト無し：{battleDebugData.NoCost}"));
                            }
                            if (Keyboard.current.oKey.wasPressedThisFrame)
                            {
                                battleDebugData.IsBigDamage = !battleDebugData.IsBigDamage;
                                gameEvents.OnRequestShowMessage.OnNext(new($"大ダメージ：{battleDebugData.IsBigDamage}"));
                            }
                            if (Keyboard.current.pKey.wasPressedThisFrame)
                            {
                                player.GrowthParameter.BehaviourPoint += 1;
                                gameEvents.OnRequestShowMessage.OnNext(new($"行動ポイント：{player.GrowthParameter.BehaviourPoint}"));
                            }
                            if (Keyboard.current.aKey.wasPressedThisFrame)
                            {
                                dungeonController.Setup(debugDungeonName, player);
                                explorationView.CreateDungeonViews();
                            }
                            if (Keyboard.current.f2Key.wasPressedThisFrame)
                            {
                                GameDebugWindowView.OpenAsync(gameMenuBundlePrefab, destroyCancellationToken).Forget();
                            }
                        }
                        catch (OperationCanceledException)
                        {
                        }
                    })
                    .RegisterTo(destroyCancellationToken);
#endif

                await destroyCancellationToken.WaitUntilCanceled();

                player.Dispose();
            }
            catch (OperationCanceledException e)
            {
                Debug.Log(e);
            }
        }
    }
}
