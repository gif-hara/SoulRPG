using HK;
using SoulRPG.CharacterControllers;
using UnityEngine;

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
        private HKUIDocument dungeonDocumentPrefab;

        [SerializeField]
        private string debugDungeonName;

        [SerializeField]
        private Vector2Int debugPosition;

        async void Start()
        {
            await BootSystem.IsReady;
            TinyServiceLocator.Register(masterData);
            var player = new Character();
            var gameCameraController = Instantiate(gameCameraControllerPrefab);
            var gameView = new GameView(
                gameUIDocumentPrefab,
                dungeonDocumentPrefab,
                gameCameraController,
                player
                );
            var playerController = new PlayerController();
            var inputController = new InputController();
            TinyServiceLocator.Register(inputController);
            var dungeonController = new DungeonController();
            TinyServiceLocator.Register(dungeonController);
            TinyServiceLocator.Register(new UserData());
            dungeonController.Setup(debugDungeonName);
            playerController.Attach(player, destroyCancellationToken);
            gameView.Open(destroyCancellationToken);
        }
    }
}
