using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG.BattleSystems.BattleCharacterEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class ContainsAilment : IBattleCharacterEvaluatorBoolean
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeField]
        private List<int> ailments;

        public bool Evaluate(BattleCharacter actor, BattleCharacter target, Container container)
        {
            var t = targetType == Define.TargetType.Self ? actor : target;
            return ailments.Any(ailment => t.AilmentController.Contains(ailment));
        }
    }
}
