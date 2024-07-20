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
                    var v = x.ReadValue<Vector2>();
                    var velocity = new Vector2Int((int)v.x, (int)v.y);
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
