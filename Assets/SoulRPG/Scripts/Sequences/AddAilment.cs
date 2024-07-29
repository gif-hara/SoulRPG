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
            var name = targetType == Define.TargetType.Self ? "Actor" : "Target";
            var target = container.Resolve<BattleCharacter>(name);
            if (target.BattleStatus.IsDead)
            {
                return;
            }
            if (battleCharacterEvaluator == null || battleCharacterEvaluator != null && battleCharacterEvaluator.Evaluate(target))
            {
                await target.AilmentController.AddAsync(masterDataAilmentId, turnCount);
            }
        }
    }
}
