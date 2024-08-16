using System;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.BattleSystems;
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
        private string debugDungeonName;

        [SerializeField]
        private Vector2Int debugPosition;

        [SerializeField]
        private bool isUseDebugPosition;

        [SerializeField]
        private string debugPlayerName;

        [SerializeField]
        private CharacterGrowthParameter debugPlayerGrowthParameter;

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
            var player = new Character(debugPlayerName, debugPlayerGrowthParameter, gameRule.InitialEquipment, debugPlayerAttribute);
            var gameCameraController = Instantiate(gameCameraControllerPrefab);
            var explorationView = new ExplorationView(
                gameUIDocumentPrefab,
                dungeonDocumentPrefab,
                gameCameraController,
                player
                );
            var playerController = new PlayerController();
            TinyServiceLocator.Register(new InputScheme(destroyCancellationToken));
            var inputController = new InputController();
            inputController.PushInputType(InputController.InputType.InGame);
            TinyServiceLocator.Register(inputController);
            var dungeonController = new DungeonController(
                gameMenuBundlePrefab,
                explorationView,
                destroyCancellationToken
                );
            TinyServiceLocator.Register(dungeonController);
            dungeonController.Setup(debugDungeonName, player);
            if (isUseDebugPosition)
            {
                player.Warp(debugPosition);
            }
            foreach (var i in gameRule.InitialItemDatabase)
            {
                player.Inventory.Add(i.ItemId, i.Count);
            }
            playerController.Attach(player, gameMenuBundlePrefab, destroyCancellationToken);
            explorationView.Open(destroyCancellationToken);
            Observable.Merge
            (
                gameEvents.OnRequestPlaySfx,
                gameEvents.OnRequestShowMessage.Select(x => x.SfxName)
            )
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
            var inputGuideCenter = new GameInputGuideView(gameUIDocumentPrefab.Q<HKUIDocument>("UI.Game.InputGuide.Center"), destroyCancellationToken);
            gameEvents.OnRequestShowInputGuideCenter
                .Subscribe(x =>
                {
                    inputGuideCenter.Open(x.message, x.scope);
                })
                .RegisterTo(destroyCancellationToken);
            gameEvents.OnRequestPlayBgm.OnNext("Bgm.Exploration.0");
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
