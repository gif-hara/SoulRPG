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
    public sealed class RegisterContainerBadEndCount : ISequence
    {
        [SerializeReference, SubclassSelector]
        private StringResolver keyResolver;
        
        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var saveData = SaveData.LoadSafe();
            container.Register(keyResolver.Resolve(container), saveData.playerData.badEndCount);
            return UniTask.CompletedTask;
        }
    }
}
