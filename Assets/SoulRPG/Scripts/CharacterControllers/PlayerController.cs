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
                    var value = x.ReadValue<Vector2>();
                    player.Position += new Vector2Int((int)value.x, (int)value.y);
                })
                .RegisterTo(scope);
        }
    }
}
