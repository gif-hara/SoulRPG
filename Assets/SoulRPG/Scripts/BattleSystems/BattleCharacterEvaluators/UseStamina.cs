using System;
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
            if (container.TryResolve<float>(keyResolver.Resolve(container), out var stamina))
            {
                return stamina > 0;
            }
            return false;
        }
    }
}
