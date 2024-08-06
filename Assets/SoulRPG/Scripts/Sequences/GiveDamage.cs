using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
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
        private Define.GiveDamageType giveDamageType;

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
            if (giveDamageType == Define.GiveDamageType.Direct && await t.EvaluateEvaded())
            {
                await gameEvents.ShowMessageAndWaitForSubmitInputAsync($"{t.BattleStatus.NameWithTag}は攻撃を回避した。");
                return;
            }

            container.TryResolve<MasterData.Weapon>(out var weapon);
            var damage = damageCalculator.Calculate(actor, target, weapon, targetType);
            Assert.IsNotNull(t, $"target is null targetType:{targetType}");
            t.BattleStatus.TakeDamage(damage);
            await gameEvents.ShowMessageAndWaitForSubmitInputAsync($"{t.BattleStatus.NameWithTag}に<color=#FFFF88>{damage}</color>のダメージを与えた。");
            if (giveDamageType == Define.GiveDamageType.Direct && !t.BattleStatus.IsDead)
            {
                await actor.BeginComboAsync(target, cancellationToken);
            }
        }
    }
}
