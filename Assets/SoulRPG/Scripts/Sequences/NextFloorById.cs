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
    public sealed class NextFloorById : ISequence
    {
        [SerializeReference, SubclassSelector]
        private IntResolver floorIdResolver;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            TinyServiceLocator.Resolve<DungeonController>().NextFloor(floorIdResolver.Resolve(container));
            return UniTask.CompletedTask;
        }
    }
}
