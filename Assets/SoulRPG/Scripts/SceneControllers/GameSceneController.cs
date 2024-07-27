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
        private string debugDungeonName;

        [SerializeField]
        private Vector2Int debugPosition;

        [SerializeField]
        private CharacterGrowthParameter debugPlayerGrowthParameter;

        [SerializeField]
        private CharacterBattleStatusBlueprint debugEnemyBattleStatus;

        async void Start()
        {
            await BootSystem.IsReady;
            TinyServiceLocator.Register(masterData);
            TinyServiceLocator.Register(new GameEvents());
            var player = new Character(debugPlayerGrowthParameter);
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
            var dungeonController = new DungeonController();
            TinyServiceLocator.Register(dungeonController);
            TinyServiceLocator.Register(new UserData());
            dungeonController.Setup(debugDungeonName);
            playerController.Attach(player, gameMenuDocumentPrefab, destroyCancellationToken);
            gameView.Open(destroyCancellationToken);
            var testMessageId = 0;
            Observable.EveryUpdate(destroyCancellationToken)
                .Subscribe(async _ =>
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
                            new BattleCharacter(player),
                            new BattleCharacter(new CharacterBattleStatus(debugEnemyBattleStatus)),
                            destroyCancellationToken
                            );
                        Debug.Log($"BattleResult: {battleResult}");
                    }

                    if (Keyboard.current.eKey.wasPressedThisFrame)
                    {
                        TinyServiceLocator.Resolve<GameEvents>().RequestShowMessage.OnNext($"TestMessage{testMessageId++}");
                    }
                })
                .RegisterTo(destroyCancellationToken);
        }
    }
}
