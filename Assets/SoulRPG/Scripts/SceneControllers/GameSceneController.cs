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
        private HKUIDocument gameDocumentPrefab;

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
            var gameView = new GameView(gameDocumentPrefab, gameCameraController, player);
            var playerController = new PlayerController();
            var inputActions = new InputActions();
            player.SetDungeon(masterData.Dungeons.Get(debugDungeonName), debugPosition);
            playerController.Attach(inputActions, player, destroyCancellationToken);
            gameView.Open(destroyCancellationToken);
            inputActions.Enable();
        }
    }
}
