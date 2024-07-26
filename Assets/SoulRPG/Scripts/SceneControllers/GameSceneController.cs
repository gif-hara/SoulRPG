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
        private CharacterGrowthParameter debugCharacterGrowthParameter;

        async void Start()
        {
            await BootSystem.IsReady;
            TinyServiceLocator.Register(masterData);
            var player = new Character(debugCharacterGrowthParameter);
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
            Observable.EveryUpdate(destroyCancellationToken)
                .Subscribe(_ =>
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
                        var battleSystem = new BattleSystem();
                        battleSystem.BeginAsync(destroyCancellationToken).Forget();
                    }
                })
                .RegisterTo(destroyCancellationToken);
        }
    }
}
