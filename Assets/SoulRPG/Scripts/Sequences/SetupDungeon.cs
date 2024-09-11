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
    public sealed class SetupDungeon : ISequence
    {
        [SerializeReference, SubclassSelector]
        private StringResolver dungeonNameResolver;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            TinyServiceLocator.Resolve<DungeonController>().Setup(
                dungeonNameResolver.Resolve(container),
                TinyServiceLocator.Resolve<Character>("Player")
            );
            return UniTask.CompletedTask;
        }
    }
}
