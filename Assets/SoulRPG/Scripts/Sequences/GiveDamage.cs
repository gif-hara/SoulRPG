using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.BattleSystems.DamageCalculators;
using UnityEngine;
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
            var target = container.Resolve<BattleCharacter>(targetType == Define.TargetType.Self ? "Actor" : "Target");
            if (target.BattleStatus.IsDead)
            {
                return;
            }
            container.TryResolve<BattleCharacter>("Actor", out var actor);
            container.TryResolve<MasterData.Weapon>(out var weapon);
            var damage = damageCalculator.Calculate(actor, target, weapon);
            target.BattleStatus.TakeDamage(damage);
            var message = $"{target.BattleStatus.Name}に{damage}のダメージ！";
            TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext(message);
            if (waitForInput)
            {
                await TinyServiceLocator.Resolve<GameEvents>().WaitForSubmitInputAsync();
            }
            if (canCombo)
            {
                await actor.OnGivedDamageAsync(target, cancellationToken);
            }
        }
    }
}
