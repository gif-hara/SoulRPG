using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
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

        [SerializeField]
        private bool ignoreIfCharacterDead = false;

        public async UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            container.TryResolve<BattleCharacter>("Actor", out var actor);
            container.TryResolve<BattleCharacter>("Target", out var target);
            if (ignoreIfCharacterDead)
            {
                if (actor != null && actor.BattleStatus.IsDead)
                {
                    return;
                }
                if (target != null && target.BattleStatus.IsDead)
                {
                    return;
                }
            }
            var message = format;
            if (actor != null)
            {
                message = message.Replace("{Actor}", actor.BattleStatus.NameWithTag);
            }
            if (target != null)
            {
                message = message.Replace("{Target}", target.BattleStatus.NameWithTag);
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
