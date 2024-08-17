using System;
using UnityEngine;

namespace SoulRPG.BattleSystems.BattleCharacterEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class ConstantFloat : IBattleCharacterEvaluatorFloat
    {
        [SerializeField]
        private float value;

        public float Evaluate(BattleCharacter actor, BattleCharacter target)
        {
            return value;
        }
    }
}
