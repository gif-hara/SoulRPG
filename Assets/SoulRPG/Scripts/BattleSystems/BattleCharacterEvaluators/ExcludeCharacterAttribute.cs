using System;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG.BattleSystems.BattleCharacterEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class ExcludeCharacterAttribute : IBattleCharacterEvaluatorBoolean
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeField]
        private Define.CharacterAttribute blackList;

        public bool Evaluate(BattleCharacter actor, BattleCharacter target, Container container)
        {
            var t = targetType == Define.TargetType.Self ? actor : target;
            return (t.BattleStatus.Attribute & blackList) == 0;
        }
    }
}
