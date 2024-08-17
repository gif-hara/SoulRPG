using System;
using System.Collections.Generic;
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
    public sealed class AddStatusBuff : ISequence
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeField]
        private List<Define.StatusType> statusTypes;

        [SerializeField]
        private string buffName;

        [SerializeReference, SubclassSelector]
        private IBattleCharacterEvaluatorFloat rateSelector;

        [SerializeReference, SubclassSelector]
        private IBattleCharacterEvaluatorBoolean battleCharacterEvaluator;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            container.TryResolve<BattleCharacter>("Actor", out var actor);
            container.TryResolve<BattleCharacter>("Target", out var target);
            if (battleCharacterEvaluator == null ||
                battleCharacterEvaluator != null && battleCharacterEvaluator.Evaluate(actor, target))
            {
                var t = targetType == Define.TargetType.Self ? actor : target;
                t.StatusBuffController.Add(statusTypes, buffName, rateSelector.Evaluate(actor, target));
            }
            return UniTask.CompletedTask;
        }
    }
}
