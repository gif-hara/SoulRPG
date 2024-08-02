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
        private Define.CharacterAttribute blackList;

        public bool Evaluate(BattleCharacter battleCharacter)
        {
            return (battleCharacter.BattleStatus.Attribute & blackList) == 0;
        }
    }
}
