using System;
using UnityEngine;

namespace SoulRPG.BattleSystems.BattleCharacterEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class ContainsDebuff : IBattleCharacterEvaluatorBoolean
    {
        [SerializeField]
        private Define.TargetType targetType;
        
        public bool Evaluate(BattleCharacter actor, BattleCharacter target)
        {
            var t = targetType == Define.TargetType.Self ? actor : target;
            return t.AilmentController.ContainsDebuff();
        }
    }
}
