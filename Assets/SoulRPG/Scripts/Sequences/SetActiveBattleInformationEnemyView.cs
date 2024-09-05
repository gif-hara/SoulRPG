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
    public sealed class SetActiveBattleInformationEnemyView : ISequence
    {
        [SerializeReference, SubclassSelector]
        private BooleanResolver activeResolver;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var view = TinyServiceLocator.Resolve<BattleInformationEnemyView>();
            view.SetActive(activeResolver.Resolve(container));
            return UniTask.CompletedTask;
        }
    }
}
