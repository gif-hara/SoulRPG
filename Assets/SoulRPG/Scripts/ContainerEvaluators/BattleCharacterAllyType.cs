using System;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG.ContainerEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class BattleCharacterAllyType : IContainerEvaluator
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeField]
        private Define.AllyType allyType;

        public bool Evaluate(Container container)
        {
            var actor = container.Resolve<BattleCharacter>("Actor");
            var target = container.Resolve<BattleCharacter>("Target");
            var t = targetType == Define.TargetType.Self ? actor : target;
            return t.BattleStatus.AllyType == allyType;
        }
    }
}
