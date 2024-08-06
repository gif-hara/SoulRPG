using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.BattleSystems.DamageCalculators;
using UnityEngine;
using UnityEngine.Assertions;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class GiveDamage : ISequence
    {
        [SerializeReference, SubclassSelector]
        private IDamageCalculator damageCalculator;

        [SerializeField]
        private Define.TargetType targetType;

        [SerializeField]
        private bool waitForInput = true;

        [SerializeField]
        private bool canCombo = true;

        public async UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            container.TryResolve<BattleCharacter>("Actor", out var actor);
            container.TryResolve<BattleCharacter>("Target", out var target);
            if ((actor != null && actor.BattleStatus.IsDead) || (target != null && target.BattleStatus.IsDead))
            {
                return;
            }
            container.TryResolve<MasterData.Weapon>(out var weapon);
            var damage = damageCalculator.Calculate(actor, target, weapon, targetType);
            var t = targetType == Define.TargetType.Self ? actor : target;
            Assert.IsNotNull(t, $"target is null targetType:{targetType}");
            t.BattleStatus.TakeDamage(damage);
            var message = $"{t.BattleStatus.NameWithTag}に<color=#FFFF88>{damage}</color>のダメージを与えた。";
            TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext(message);
            if (waitForInput)
            {
                await TinyServiceLocator.Resolve<GameEvents>().WaitForSubmitInputAsync();
            }
            if (canCombo && !target.BattleStatus.IsDead)
            {
                await actor.BeginComboAsync(target, cancellationToken);
            }
        }
    }
}
