using System;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.CharacterControllers;
using UnityEngine;
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
        private string debugPlayerName;

        [SerializeField]
        private Define.CharacterAttribute debugPlayerAttribute;

        [SerializeField]
        private int debugEnemyMasterDataId;

        [SerializeField]
        private CharacterBattleStatusBlueprint debugEnemyBattleStatus;

        async void Start()
        {
            await BootSystem.IsReady;
            TinyServiceLocator.Register(masterData);
            var gameEvents = new GameEvents();
            TinyServiceLocator.Register(gameEvents);
            TinyServiceLocator.Register(gameRule);
#if DEBUG
            GameDebugPanelView.OpenAsync(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.DebugPanel"), destroyCancellationToken).Forget();
#endif
            TinyServiceLocator.Register(new GameFadeView(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.Fade"), destroyCancellationToken));
            var player = new Character(debugPlayerName, gameRule.PlayerGrowthParameter, gameRule.InitialEquipment, debugPlayerAttribute);
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
            if (isUseDebugPosition)
            {
                player.Warp(debugPosition);
            }
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
                    AudioManager.PlaySFX(gameRule.AudioDatabase.Get(x).Clip);
                })
                .RegisterTo(destroyCancellationToken);
            gameEvents.OnRequestPlayBgm
                .Subscribe(x =>
                {
                    AudioManager.PlayBGM(gameRule.AudioDatabase.Get(x).Clip);
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
            gameEvents.OnRequestPlayBgm.OnNext("Bgm.Exploration.0");
            var screenEffectView = new ScreenEffectView(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.ScreenEffect"), destroyCancellationToken);
            TinyServiceLocator.Register(screenEffectView);
            inputController.PushInputType(InputController.InputType.InGame);
            var gameTipsView = new GameTipsView(gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.Tips"), destroyCancellationToken);
            TinyServiceLocator.Register(gameTipsView);
            gameEvents.OnRequestChangeDungeon.OnNext(debugDungeonName);
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
                            player.InstanceStatus.AddExperience(100000);
                            TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext(new("[DEBUG] Add Experience 100000", "Sfx.Message.0"));
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
    }
}
