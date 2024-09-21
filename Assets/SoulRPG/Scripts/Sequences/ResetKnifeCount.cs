using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using SoulRPG.BattleSystems.BattleCharacterEvaluators;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class ResetKnifeCount : ISequence
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeReference, SubclassSelector]
        private IBattleCharacterEvaluatorBoolean battleCharacterEvaluator;

        public async UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var actor = container.Resolve<BattleCharacter>("Actor");
            var target = container.Resolve<BattleCharacter>("Target");
            if (actor.BattleStatus.IsDead || target.BattleStatus.IsDead)
            {
                return;
            }
            if (battleCharacterEvaluator == null || battleCharacterEvaluator != null && battleCharacterEvaluator.Evaluate(actor, target, container))
            {
                var t = targetType == Define.TargetType.Self ? actor : target;
                t.BattleStatus.ResetKnifeCount();
                var message = "{0}のナイフストックがリセットされた。".Localized().Format(t.BattleStatus.NameWithTag);
                await TinyServiceLocator.Resolve<GameEvents>().ShowMessageAndWaitForSubmitInputAsync(new(message, "Sfx.Message.0"));
            }
        }
    }
}
