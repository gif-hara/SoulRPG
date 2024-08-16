using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.CharacterControllers;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class PlayerController
    {
        public void Attach(Character player, HKUIDocument gameMenuBundlePrefab, CancellationToken scope)
        {
            var inputController = TinyServiceLocator.Resolve<InputController>();
            var inGameActions = inputController.InputActions.InGame;
            var dungeonController = TinyServiceLocator.Resolve<DungeonController>();
            dungeonController.EnterAsync(player).Forget();
            inGameActions.Move.OnPerformedAsObservable()
                .Subscribe(x =>
                {
                    var velocity = x.ReadValue<Vector2>().ToVector2Int();
                    if (inGameActions.Shift.IsPressed())
                    {
                        if (velocity == Vector2Int.zero)
                        {
                            return;
                        }
                        velocity = player.Direction.TransformVelocityByDirection(velocity);
                        player.Move(velocity);
                        dungeonController.EnterAsync(player).Forget();
                    }
                    else
                    {
                        if (!velocity.CanConvertToDirection())
                        {
                            return;
                        }
                        if (velocity.y == 1 || velocity.y == -1)
                        {
                            velocity = player.Direction.TransformVelocityByDirection(velocity);
                            player.Move(velocity);
                            dungeonController.EnterAsync(player).Forget();
                        }
                        else if (velocity.x == 1 || velocity.x == -1)
                        {
                            var direction = velocity.ToDirection();
                            player.Direction = player.Direction.Rotate(direction);
                            dungeonController.EnterAsync(player).Forget();
                        }
                    }
                })
                .RegisterTo(scope);
            inGameActions.Interact.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    dungeonController.InteractAsync(player).Forget();
                })
                .RegisterTo(scope);
            inGameActions.ToMenu.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    var view = new GamePauseMenuView(gameMenuBundlePrefab, player);
                    view.OpenAsync().Forget();
                })
                .RegisterTo(scope);
        }
    }
}
