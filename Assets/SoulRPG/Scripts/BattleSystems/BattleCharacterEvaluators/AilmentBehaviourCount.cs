using System;
using SoulRPG.ContainerEvaluators;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG.BattleSystems.BattleCharacterEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class AilmentBehaviourCount : IBattleCharacterEvaluatorBoolean
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeField]
        private int masterDataAilmentId;

        [SerializeField]
        private int behaviourCount;

        [SerializeField]
        private IContainerEvaluator.CompareType compareType;

        public bool Evaluate(BattleCharacter actor, BattleCharacter target, Container container)
        {
            var t = targetType == Define.TargetType.Self ? actor : target;
            var ailmentElement = t.AilmentController.Find(masterDataAilmentId);
            if (ailmentElement == null)
            {
                return false;
            }

            var count = ailmentElement.GetBehaviourCount();
            return compareType switch
            {
                IContainerEvaluator.CompareType.Equals => count == behaviourCount,
                IContainerEvaluator.CompareType.GreaterThan => count > behaviourCount,
                IContainerEvaluator.CompareType.GreaterThanOrEquals => count >= behaviourCount,
                IContainerEvaluator.CompareType.LessThan => count < behaviourCount,
                IContainerEvaluator.CompareType.LessThanOrEquals => count <= behaviourCount,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
