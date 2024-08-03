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
        private string debugPlayerName;

        [SerializeField]
        private CharacterGrowthParameter debugPlayerGrowthParameter;

        [SerializeField]
        private EquipmentBlueprint debugPlayerEquipment;

        [SerializeField]
        private Define.CharacterAttribute debugPlayerAttribute;

        [SerializeField]
        private CharacterBattleStatusBlueprint debugEnemyBattleStatus;

        async void Start()
        {
            await BootSystem.IsReady;
            TinyServiceLocator.Register(masterData);
            TinyServiceLocator.Register(new GameEvents());
            TinyServiceLocator.Register(gameRule);
            var player = new Character(debugPlayerName, debugPlayerGrowthParameter, debugPlayerEquipment, debugPlayerAttribute);
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
                player.Position,
                gameMenuBundlePrefab,
                explorationView,
                destroyCancellationToken
                );
            TinyServiceLocator.Register(dungeonController);
            TinyServiceLocator.Register(new UserData());
            dungeonController.Setup(debugDungeonName);
            playerController.Attach(player, gameMenuBundlePrefab, destroyCancellationToken);
            explorationView.Open(destroyCancellationToken);
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
                            TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext("[DEBUG] Add All Items");
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
                            TinyServiceLocator.Resolve<UserData>().AddExperience(100000);
                            TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext("[DEBUG] Add Experience 100000");
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
