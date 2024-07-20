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
        private HKUIDocument gameDocumentPrefab;

        async void Start()
        {
            await BootSystem.IsReady;
            var player = new Character();
            var gameView = new GameView(gameDocumentPrefab, player);
            var playerController = new PlayerController();
            var inputActions = new InputActions();
            playerController.Attach(inputActions, player, destroyCancellationToken);
            gameView.Open(destroyCancellationToken);
            inputActions.Enable();
        }
    }
}
