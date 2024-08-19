using System;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG.ContainerEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class ContainsDebuff : IContainerEvaluator
    {
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
            return actor.AilmentController.ContainsDebuff();
        }
    }
}