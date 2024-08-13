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
    public sealed class ChangeEnemySprite : ISequence
    {
        [SerializeField]
        private Sprite sprite;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            gameEvents.OnRequestChangeEnemySprite.OnNext(sprite);
            return UniTask.CompletedTask;
        }
    }
}
