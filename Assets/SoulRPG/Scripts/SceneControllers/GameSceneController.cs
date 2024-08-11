using System;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.BattleSystems;
using SoulRPG.CharacterControllers;
using Unity.VisualScripting;
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
            var player = new Character(debugPlayerName, debugPlayerGrowthParameter, gameRule.InitialEquipment, debugPlayerAttribute);
            var gameCameraController = Instantiate(gameCameraControllerPrefab);
            var explorationView = new ExplorationView(
                gameUIDocumentPrefab,
                dungeonDocumentPrefab,
                gameCameraController,
                player
                );
            var playerController = new PlayerController();
            var inputController = new InputController();
            inputController.ChangeInputType(InputController.InputType.InGame);
            TinyServiceLocator.Register(inputController);
            var dungeonController = new DungeonController(
                gameMenuBundlePrefab,
                explorationView
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
            Observable.EveryUpdate(destroyCancellationToken)
                .Subscribe(async _ =>
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
                            var battleResult = await BattleSystem.BeginAsync(
                                new BattleCharacter(player, Define.AllyType.Player, new Input(commandDocumentPrefab)),
                                new BattleCharacter(new CharacterBattleStatus(debugEnemyBattleStatus), new Constant(101998, 101001, false)),
                                destroyCancellationToken
                                );
                            Debug.Log($"BattleResult: {battleResult}");
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
                    }
                    catch (OperationCanceledException)
                    {
                    }
                })
                .RegisterTo(destroyCancellationToken);

            await destroyCancellationToken.WaitUntilCanceled();

            player.Dispose();
        }
    }
}
