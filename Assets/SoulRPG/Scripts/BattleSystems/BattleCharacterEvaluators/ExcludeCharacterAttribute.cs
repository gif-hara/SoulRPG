using System;
using UnityEngine;

namespace SoulRPG.BattleSystems.BattleCharacterEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class ExcludeCharacterAttribute : IBattleCharacterEvaluator
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeField]
        private Define.CharacterAttribute blackList;

        public bool Evaluate(BattleCharacter actor, BattleCharacter target)
        {
            var t = targetType == Define.TargetType.Self ? actor : target;
            return !t.BattleStatus.Attribute.HasFlag(blackList);
        }
    }
}
