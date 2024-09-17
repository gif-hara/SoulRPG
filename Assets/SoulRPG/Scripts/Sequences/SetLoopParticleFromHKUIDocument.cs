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
    public sealed class SetLoopParticleFromHKUIDocument : ISequence
    {
        [SerializeReference, SubclassSelector]
        private StringResolver documentNameResolver;

        [SerializeReference, SubclassSelector]
        private StringResolver particleNameResolver;

        [SerializeReference, SubclassSelector]
        private BooleanResolver loopResolver;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var documentName = documentNameResolver.Resolve(container);
            if (container.TryResolve<HKUIDocument>(documentName, out var document))
            {
                var main = document.Q<ParticleSystem>(particleNameResolver.Resolve(container)).main;
                main.loop = loopResolver.Resolve(container);
            }
            else
            {
                Debug.LogWarning($"Document is not found. {documentName}");
            }
            return UniTask.CompletedTask;
        }
    }
}
