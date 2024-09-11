using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using SoulRPG.CharacterControllers;
using UnityEngine;
using UnitySequencerSystem;
using UnitySequencerSystem.Resolvers;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class ResetAllPlayer : ISequence
    {
        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var player = TinyServiceLocator.Resolve<Character>("Player");
            player.ResetAll();
            return UniTask.CompletedTask;
        }
    }
}
