using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using SoulRPG.BattleSystems.RecoveryCalculators;
using UnityEngine;
using UnityEngine.Assertions;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class RecoveryHitPoint : ISequence
    {
        [SerializeReference, SubclassSelector]
        private IRecoveryCalculator recoveryCalculator;

        [SerializeField]
        private Define.TargetType targetType;

        [SerializeField]
        private string sfxName = "Sfx.Message.19";

        public async UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            container.TryResolve<BattleCharacter>("Actor", out var actor);
            container.TryResolve<BattleCharacter>("Target", out var target);
            if ((actor != null && actor.BattleStatus.IsDead) || (target != null && target.BattleStatus.IsDead))
            {
                return;
            }
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            var t = targetType == Define.TargetType.Self ? actor : target;
            Assert.IsNotNull(t, $"target is null targetType:{targetType}");
            var fixedSfxName = string.IsNullOrEmpty(sfxName) ? "Sfx.Message.19" : sfxName;
            var recovery = recoveryCalculator.Calculate(container);
            if (recovery <= 0)
            {
                return;
            }
            else
            {
                Assert.IsNotNull(t, $"target is null targetType:{targetType}");
                var message = "{0}のHPが<color=#88FF88>{1}</color>回復した。".Localized().Format(t.BattleStatus.NameWithTag, recovery);
                await UniTask.WhenAll(
                    t.RecoveryHitPointAsync(recovery),
                    gameEvents.ShowMessageAndWaitForSubmitInputAsync(new(message, fixedSfxName))
                );
            }
        }
    }
}
