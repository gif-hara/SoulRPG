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
    public sealed class PlaySfx : ISequence
    {
        [SerializeField]
        private string sfxName;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            gameEvents.OnRequestPlaySfx.OnNext(sfxName);
            return UniTask.CompletedTask;
        }
    }
}
