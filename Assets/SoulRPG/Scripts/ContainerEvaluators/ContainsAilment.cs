using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG.ContainerEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class ContainsAilment : IContainerEvaluator
    {
        [SerializeField]
        private List<int> ailmentMasterDataIds;

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
            return ailmentMasterDataIds.Any(ailmentMasterDataId => actor.AilmentController.Contains(ailmentMasterDataId));
        }
    }
}
