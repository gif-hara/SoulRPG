using System;
using SoulRPG.ContainerEvaluators;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG.SequenceSystems.Evaluates
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class HitPoint : IEvaluate
    {
        [SerializeField]
        [Range(0, 1)]
        private float rate;

        [SerializeField]
        private IContainerEvaluator.CompareType compareType;

        [SerializeField]
        private Define.TargetType targetType;

        public bool Evaluate(Container container)
        {
            var key = targetType switch
            {
                Define.TargetType.Self => "Actor",
                Define.TargetType.Opponent => "Target",
                _ => throw new ArgumentOutOfRangeException()
            };
            var actor = container.Resolve<BattleCharacter>(key);
            return compareType.Compare(actor.BattleStatus.HitPointRate, rate);
        }
    }
}
