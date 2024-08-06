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
    public sealed class AddAilment : ISequence
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeField]
        private int masterDataAilmentId;

        [SerializeField]
        private int turnCount;

        [SerializeReference, SubclassSelector]
        private IBattleCharacterEvaluator battleCharacterEvaluator;

        public async UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            container.TryResolve<BattleCharacter>("Actor", out var actor);
            container.TryResolve<BattleCharacter>("Target", out var target);
            if ((actor != null && actor.BattleStatus.IsDead) || (target != null && target.BattleStatus.IsDead))
            {
                return;
            }
            if (battleCharacterEvaluator == null || battleCharacterEvaluator != null && battleCharacterEvaluator.Evaluate(actor, target))
            {
                var t = targetType == Define.TargetType.Self ? actor : target;
                await t.AilmentController.AddAsync(masterDataAilmentId, turnCount);
            }
        }
    }
}
