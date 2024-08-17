using System;
using UnityEngine;

namespace SoulRPG.BattleSystems.BattleCharacterEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class Random : IBattleCharacterEvaluatorBoolean
    {
        [SerializeField]
        [Range(0, 1)]
        private float probability;

        public bool Evaluate(BattleCharacter actor, BattleCharacter target)
        {
            return UnityEngine.Random.value < probability;
        }
    }
}
