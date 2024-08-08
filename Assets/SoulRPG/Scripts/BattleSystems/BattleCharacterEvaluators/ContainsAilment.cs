using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SoulRPG.BattleSystems.BattleCharacterEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class ContainsAilment : IBattleCharacterEvaluator
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeField]
        private List<int> ailments;

        public bool Evaluate(BattleCharacter actor, BattleCharacter target)
        {
            var t = targetType == Define.TargetType.Self ? actor : target;
            return ailments.Any(ailment => t.AilmentController.Contains(ailment));
        }
    }
}
