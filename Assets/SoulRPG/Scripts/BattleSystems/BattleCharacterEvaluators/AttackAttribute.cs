using System;
using System.Collections.Generic;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG.BattleSystems.BattleCharacterEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class AttackAttribute : IBattleCharacterEvaluatorBoolean
    {
        [SerializeField]
        private List<Define.AttackAttribute> attackAttributes;

        [SerializeField]
        private string key;

        public bool Evaluate(BattleCharacter actor, BattleCharacter target, Container container)
        {
            var targetAttackAttribute = container.Resolve<Define.AttackAttribute>(key);
            return attackAttributes.Contains(targetAttackAttribute);
        }
    }
}
