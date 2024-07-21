using System.Threading;
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
        public void Attach(InputActions inputActions, Character player, CancellationToken scope)
        {
            inputActions.InGame.Move.OnPerformedAsObservable()
                .Subscribe(x =>
                {
                    var velocity = x.ReadValue<Vector2>().ToVector2Int();
                    if (inputActions.InGame.Shift.IsPressed())
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
                        velocity = player.Direction.TransformVelocityByDirection(velocity);
                        player.Position += new Vector2Int(velocity.x, velocity.y);
                    }
                })
                .RegisterTo(scope);
        }
    }
}
