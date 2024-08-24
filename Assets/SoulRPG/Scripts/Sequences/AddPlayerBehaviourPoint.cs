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
    public sealed class AddPlayerBehaviourPoint : ISequence
    {
        [SerializeReference, SubclassSelector]
        private IntResolver valueResolver;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            TinyServiceLocator.Resolve<Character>("Player").GrowthParameter.BehaviourPoint += valueResolver.Resolve(container);
            return UniTask.CompletedTask;
        }
    }
}
