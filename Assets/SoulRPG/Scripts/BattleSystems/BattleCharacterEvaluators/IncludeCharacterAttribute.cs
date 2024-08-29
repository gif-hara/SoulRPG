using System;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG.BattleSystems.BattleCharacterEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class IncludeCharacterAttribute : IBattleCharacterEvaluatorBoolean
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeField]
        private Define.CharacterAttribute whiteList;

        public bool Evaluate(BattleCharacter actor, BattleCharacter target, Container container)
        {
            var t = targetType == Define.TargetType.Self ? actor : target;
            return (t.BattleStatus.Attribute & whiteList) != 0;
        }
    }
}
