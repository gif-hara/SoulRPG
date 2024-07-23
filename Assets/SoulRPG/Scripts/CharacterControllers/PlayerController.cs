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
        public void Attach(Character player, HKUIDocument gameMenuDocumentPrefab, CancellationToken scope)
        {
            var inputController = TinyServiceLocator.Resolve<InputController>();
            var inGameActions = inputController.InputActions.InGame;
            inGameActions.Move.OnPerformedAsObservable()
                .Subscribe(x =>
                {
                    var velocity = x.ReadValue<Vector2>().ToVector2Int();
                    if (inGameActions.Shift.IsPressed())
                    {
                        if (!velocity.CanConvertToDirection())
                        {
                            return;
                        }
                        var direction = velocity.ToDirection();
                        player.Direction = player.Direction.Rotate(direction);
                    }
                    else
                    {
                        if (velocity == Vector2Int.zero)
                        {
                            return;
                        }
                        velocity = player.Direction.TransformVelocityByDirection(velocity);
                        player.Move(velocity);
                    }
                })
                .RegisterTo(scope);
            inGameActions.Interact.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    TinyServiceLocator.Resolve<DungeonController>().InteractAsync(player).Forget();
                })
                .RegisterTo(scope);
            inGameActions.ToMenu.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    var gameMenuView = new GameMenuView(gameMenuDocumentPrefab, player);
                    gameMenuView.OpenAsync().Forget();
                })
                .RegisterTo(scope);
        }
    }
}
