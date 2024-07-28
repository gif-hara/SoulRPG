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
        private GameCameraController gameCameraControllerPrefab;

        [SerializeField]
        private HKUIDocument gameUIDocumentPrefab;

        [SerializeField]
        private HKUIDocument gameMenuDocumentPrefab;

        [SerializeField]
        private HKUIDocument dungeonDocumentPrefab;

        [SerializeField]
        private HKUIDocument commandDocumentPrefab;

        [SerializeField]
        private string debugDungeonName;

        [SerializeField]
        private Vector2Int debugPosition;

        [SerializeField]
        private string debugPlayerName;

        [SerializeField]
        private CharacterGrowthParameter debugPlayerGrowthParameter;

        [SerializeField]
        private CharacterBattleStatusBlueprint debugEnemyBattleStatus;

        async void Start()
        {
            await BootSystem.IsReady;
            TinyServiceLocator.Register(masterData);
            TinyServiceLocator.Register(new GameEvents());
            var player = new Character(debugPlayerName, debugPlayerGrowthParameter);
            var gameCameraController = Instantiate(gameCameraControllerPrefab);
            var gameView = new ExplorationView(
                gameUIDocumentPrefab,
                dungeonDocumentPrefab,
                gameCameraController,
                player
                );
            var playerController = new PlayerController();
            var inputController = new InputController();
            inputController.ChangeInputType(InputController.InputType.InGame);
            TinyServiceLocator.Register(inputController);
            var dungeonController = new DungeonController(player.Position, commandDocumentPrefab, destroyCancellationToken);
            TinyServiceLocator.Register(dungeonController);
            TinyServiceLocator.Register(new UserData());
            dungeonController.Setup(debugDungeonName);
            playerController.Attach(player, gameMenuDocumentPrefab, destroyCancellationToken);
            gameView.Open(destroyCancellationToken);
            var testMessageId = 0;
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
                            Debug.Log("Add All Items");
                        }

                        if (Keyboard.current.wKey.wasPressedThisFrame)
                        {
                            var battleResult = await BattleSystem.BeginAsync(
                                new BattleCharacter(player, new Input(commandDocumentPrefab)),
                                new BattleCharacter(new CharacterBattleStatus(debugEnemyBattleStatus), new Constant(101998, 101001)),
                                destroyCancellationToken
                                );
                            Debug.Log($"BattleResult: {battleResult}");
                        }

                        if (Keyboard.current.eKey.wasPressedThisFrame)
                        {
                            TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext($"TestMessage{testMessageId++}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                })
                .RegisterTo(destroyCancellationToken);
        }
    }
}
