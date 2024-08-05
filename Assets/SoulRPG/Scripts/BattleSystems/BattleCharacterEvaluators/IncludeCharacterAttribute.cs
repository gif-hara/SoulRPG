using System;
using UnityEngine;

namespace SoulRPG.BattleSystems.BattleCharacterEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class IncludeCharacterAttribute : IBattleCharacterEvaluator
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeField]
        private Define.CharacterAttribute whiteList;

        public bool Evaluate(BattleCharacter actor, BattleCharacter target)
        {
            var t = targetType == Define.TargetType.Self ? actor : target;
            return (t.BattleStatus.Attribute & whiteList) != 0;
        }
    }
}
