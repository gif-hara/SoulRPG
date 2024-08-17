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
    public sealed class BeginAnimationGameEnemyView : ISequence
    {
        [SerializeField]
        private string animationName;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            return TinyServiceLocator.Resolve<GameEnemyView>().BeginAnimationAsync(animationName);
        }
    }
}
