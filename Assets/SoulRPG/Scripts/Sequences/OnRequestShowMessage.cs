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

        public async UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var message = format
                .Replace("{Actor}", container.Resolve<BattleCharacter>("Actor").BattleStatus.Name)
                .Replace("{Target}", container.Resolve<BattleCharacter>("Target").BattleStatus.Name);
            TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext(message);
            await UniTask.Delay(1000, cancellationToken: cancellationToken);
        }
    }
}
