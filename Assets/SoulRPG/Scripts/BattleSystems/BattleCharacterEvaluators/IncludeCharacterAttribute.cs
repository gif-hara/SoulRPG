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
        private Define.CharacterAttribute whiteList;

        public bool Evaluate(BattleCharacter battleCharacter)
        {
            return (battleCharacter.BattleStatus.Attribute & whiteList) != 0;
        }
    }
}
