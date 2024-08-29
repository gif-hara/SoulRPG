using System;
using System.Linq;
using UnityEngine;
using UnitySequencerSystem;
using UnitySequencerSystem.Resolvers;

namespace SoulRPG.BattleSystems.BattleCharacterEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class UseStamina : IBattleCharacterEvaluatorBoolean
    {
        [SerializeReference, SubclassSelector]
        private StringResolver keyResolver;

        public bool Evaluate(BattleCharacter actor, BattleCharacter target, Container container)
        {
            return container.Resolve<float>(keyResolver.Resolve(container)) > 0;
        }
    }
}
