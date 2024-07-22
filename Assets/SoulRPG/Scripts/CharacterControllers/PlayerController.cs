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
                        if (velocity == Vector2Int.zero)
                        {
                            return;
                        }
                        velocity = player.Direction.TransformVelocityByDirection(velocity);
                        player.Move(velocity);
                    }
                })
                .RegisterTo(scope);
            inputActions.InGame.Search.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    var masterData = TinyServiceLocator.Resolve<MasterData>();
                    if (masterData.DungeonEvents.TryGetValue(player, out var dungeonEvent))
                    {
                        Debug.Log($"{dungeonEvent.EventType}");
                    }
                    else
                    {
                        Debug.Log("Not Found DungeonEvent");
                    }
                })
                .RegisterTo(scope);
        }
    }
}
