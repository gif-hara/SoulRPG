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
    public sealed class AddBadEndCount : ISequence
    {
        [SerializeReference, SubclassSelector]
        private IntResolver addValueResolver;
        
        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var saveData = SaveData.LoadSafe();
            saveData.playerData.badEndCount += addValueResolver.Resolve(container);
            saveData.Save();
            return UniTask.CompletedTask;
        }
    }
}
