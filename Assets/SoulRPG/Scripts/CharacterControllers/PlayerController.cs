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
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
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
                        var moved = player.Move(velocity);
                        dungeonController.EnterAsync(player).Forget();
                        if (moved)
                        {
                            AudioManager.PlaySfx($"Sfx.Walk.{Random.Range(0, 2)}");
                        }
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
                            var moved = player.Move(velocity);
                            dungeonController.EnterAsync(player).Forget();
                            if (moved)
                            {
                                AudioManager.PlaySfx($"Sfx.Walk.{Random.Range(0, 2)}");
                            }
                        }
                        else if (velocity.x == 1 || velocity.x == -1)
                        {
                            var direction = velocity.ToDirection();
                            player.Direction = player.Direction.Rotate(direction);
                            dungeonController.EnterAsync(player).Forget();
                            AudioManager.PlaySfx($"Sfx.Walk.{Random.Range(0, 2)}");
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
            inGameActions.ToggleMiniMapView.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    gameEvents.OnRequestToggleMiniMapType.OnNext(Unit.Default);
                })
                .RegisterTo(scope);
        }
    }
}
