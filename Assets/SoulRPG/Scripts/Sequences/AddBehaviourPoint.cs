using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using SoulRPG.BattleSystems.BattleCharacterEvaluators;
using UnityEngine;
using UnityEngine.Serialization;
using UnitySequencerSystem;
using UnitySequencerSystem.Resolvers;

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
        private IBattleCharacterEvaluatorBoolean battleCharacterEvaluator;

        [SerializeReference, SubclassSelector]
        private IntResolver valueResolver;

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
                var value = valueResolver.Resolve(container);
                t.BattleStatus.AddBehaviourPoint(value);
                var message = value > 0
                    ? "{0}のBPが<color=#99FF99>{1}</color>回復した。".Localized().Format(t.BattleStatus.NameWithTag, value)
                    : "{0}のBPが<color=#FF9999>{1}</color>減少した。".Localized().Format(t.BattleStatus.NameWithTag, -value);
                await TinyServiceLocator.Resolve<GameEvents>().ShowMessageAndWaitForSubmitInputAsync(new(message, "Sfx.Message.0"));
            }
        }
    }
}
