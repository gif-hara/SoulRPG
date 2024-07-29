using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class OnRequestShowMessage : ISequence
    {
        [SerializeField]
        private string format;

        [SerializeField]
        private bool waitForInput = true;

        public async UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var message = format;
            if(container.TryResolve<BattleCharacter>("Actor", out var actor))
            {
                message = message.Replace("{Actor}", actor.BattleStatus.Name);
            }
            if(container.TryResolve<BattleCharacter>("Target", out var target))
            {
                message = message.Replace("{Target}", target.BattleStatus.Name);
            }
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            gameEvents.OnRequestShowMessage.OnNext(message);
            if (waitForInput)
            {
                await gameEvents.WaitForSubmitInputAsync();
            }
        }
    }
}
