using System;
using HK;
using UnityEngine;
using UnitySequencerSystem;
using UnitySequencerSystem.Resolvers;

namespace SoulRPG.ContainerEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class IsMatchDefaultSealedName : IContainerEvaluator
    {
        [SerializeReference, SubclassSelector]
        private BooleanResolver isTrueResolver;
        
        public bool Evaluate(Container container)
        {
            var saveData = SaveData.LoadSafe();
            var gameRule = TinyServiceLocator.Resolve<GameRule>();
            return (saveData.playerData.sealedName == gameRule.DefaultSealedName.Localized()) == isTrueResolver.Resolve(container);
        }
    }
}
