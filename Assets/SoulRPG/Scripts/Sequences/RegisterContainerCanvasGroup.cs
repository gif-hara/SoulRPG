using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnitySequencerSystem;
using UnitySequencerSystem.Resolvers;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class RegisterContainerCanvasGroup : ISequence
    {
        [SerializeReference, SubclassSelector]
        private CanvasGroupResolver targetResolver;

        [SerializeReference, SubclassSelector]
        private StringResolver keyResolver;


        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var key = keyResolver.Resolve(container);
            var target = targetResolver.Resolve(container);
            container.RegisterOrReplace(key, target);
            return UniTask.CompletedTask;
        }
    }
}
