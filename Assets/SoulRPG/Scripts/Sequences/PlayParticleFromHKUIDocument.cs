using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using UnityEngine;
using UnitySequencerSystem;
using UnitySequencerSystem.Resolvers;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class PlayParticleFromHKUIDocument : ISequence
    {
        [SerializeReference, SubclassSelector]
        private StringResolver documentNameResolver;

        [SerializeReference, SubclassSelector]
        private StringResolver particleNameResolver;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var document = container.Resolve<HKUIDocument>(documentNameResolver.Resolve(container));
            document.Q<ParticleSystem>(particleNameResolver.Resolve(container)).Play(true);
            return UniTask.CompletedTask;
        }
    }
}
