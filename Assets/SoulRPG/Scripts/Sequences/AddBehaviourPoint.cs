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
    public sealed class AddBehaviourPoint : ISequence
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeReference, SubclassSelector]
        private IBattleCharacterEvaluator battleCharacterEvaluator;

        [SerializeField]
        private int behaviourPriority;

        public async UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var actor = container.Resolve<BattleCharacter>("Actor");
            var target = container.Resolve<BattleCharacter>("Target");
            if (actor.BattleStatus.IsDead || target.BattleStatus.IsDead)
            {
                return;
            }
            if (battleCharacterEvaluator == null || battleCharacterEvaluator != null && battleCharacterEvaluator.Evaluate(actor, target))
            {
                var t = targetType == Define.TargetType.Self ? actor : target;
                t.BattleStatus.AddBehaviourPoint(behaviourPriority);
                await TinyServiceLocator.Resolve<GameEvents>().ShowMessageAndWaitForSubmitInputAsync(new($"{t.BattleStatus.NameWithTag}のBPが<color=#99FF99>{behaviourPriority}</color>回復した", "Sfx.Message.0"));
            }
        }
    }
}
