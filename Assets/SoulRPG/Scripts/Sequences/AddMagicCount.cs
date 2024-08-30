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
    public sealed class AddMagicCount : ISequence
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeReference, SubclassSelector]
        private IBattleCharacterEvaluatorBoolean battleCharacterEvaluator;

        [SerializeReference, SubclassSelector]
        private IntResolver valueResolver;

        [SerializeField]
        private bool isSilent;

        public async UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var actor = container.Resolve<BattleCharacter>("Actor");
            var target = container.Resolve<BattleCharacter>("Target");
            if (actor.BattleStatus.IsDead || target.BattleStatus.IsDead)
            {
                return;
            }

            if (battleCharacterEvaluator == null ||
                battleCharacterEvaluator != null && battleCharacterEvaluator.Evaluate(actor, target, container))
            {
                var t = targetType == Define.TargetType.Self ? actor : target;
                t.BattleStatus.AddMagicCount(valueResolver.Resolve(container));
                if (!isSilent)
                {
                    await TinyServiceLocator.Resolve<GameEvents>().ShowMessageAndWaitForSubmitInputAsync(new($"{t.BattleStatus.NameWithTag}の魔カウントが<color=#99FF99>{valueResolver}</color>蓄積した", "Sfx.Message.0"));
                }
            }
        }
    }
}
