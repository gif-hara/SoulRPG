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
    public sealed class SetIsTrueEndRoute : ISequence
    {
        [SerializeReference, SubclassSelector]
        private BooleanResolver valueResolver;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var saveData = SaveData.LoadSafe();
            saveData.playerData.isTrueEndRoute = valueResolver.Resolve(container);
            saveData.Save();
            return UniTask.CompletedTask;
        }
    }
}
