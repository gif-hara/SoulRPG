using System;
using UnityEngine;

namespace SoulRPG.BattleSystems.BattleCharacterEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class PowFromAilmentCount : IBattleCharacterEvaluatorFloat
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeField]
        private float rate;

        public float Evaluate(BattleCharacter actor, BattleCharacter target)
        {
            var t = targetType == Define.TargetType.Self ? actor : target;
            return Mathf.Pow(rate, t.AilmentController.Elements.Count);
        }
    }
}
