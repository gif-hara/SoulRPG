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
                    ? $"{t.BattleStatus.NameWithTag}のBPが<color=#99FF99>{value}</color>回復した。"
                    : $"{t.BattleStatus.NameWithTag}のBPが<color=#FF9999>{-value}</color>減少した。";
                await TinyServiceLocator.Resolve<GameEvents>().ShowMessageAndWaitForSubmitInputAsync(new(message, "Sfx.Message.0"));
            }
        }
    }
}
