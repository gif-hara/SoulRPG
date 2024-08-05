using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoulRPG.BattleSystems.BattleCharacterEvaluators;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class AddBehaviourPoint : ISequence
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeReference, SubclassSelector]
        private IBattleCharacterEvaluator battleCharacterEvaluator;

        [SerializeField]
        private int behaviourPriority;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var actor = container.Resolve<BattleCharacter>("Actor");
            var target = container.Resolve<BattleCharacter>("Target");
            if (target.BattleStatus.IsDead)
            {
                return UniTask.CompletedTask;
            }
            if (battleCharacterEvaluator == null || battleCharacterEvaluator != null && battleCharacterEvaluator.Evaluate(actor, target))
            {
                var t = targetType == Define.TargetType.Self ? actor : target;
                t.BattleStatus.AddBehaviourPoint(behaviourPriority);
            }
            return UniTask.CompletedTask;
        }
    }
}
