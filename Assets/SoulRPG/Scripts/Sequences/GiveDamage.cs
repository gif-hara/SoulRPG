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

        [SerializeField]
        private string sfxName = "Sfx.Message.2";

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
            if (giveDamageType == Define.GiveDamageType.Direct && await t.EvaluateEvaded())
            {
                await gameEvents.ShowMessageAndWaitForSubmitInputAsync(new($"{t.BattleStatus.NameWithTag}は攻撃を回避した。", "Sfx.Message.0"));
                return;
            }
            var fixedSfxName = string.IsNullOrEmpty(sfxName) ? "Sfx.Message.2" : sfxName;
            container.TryResolve<MasterData.Weapon>(out var weapon);
            var damage = damageCalculator.Calculate(actor, target, weapon, targetType, container);
#if DEBUG
            if (TinyServiceLocator.Resolve<BattleDebugData>().IsBigDamage)
            {
                damage = 99999;
            }
#endif
            container.RegisterOrReplace("Damage", damage);
            Assert.IsNotNull(t, $"target is null targetType:{targetType}");
            await UniTask.WhenAll(
                t.TakeDamageAsync(damage),
                gameEvents.ShowMessageAndWaitForSubmitInputAsync(new($"{t.BattleStatus.NameWithTag}に<color=#FFFF88>{damage}</color>のダメージを与えた。", fixedSfxName))
            );
            var a = t == actor ? target : actor;
            var attackAttribute = weapon?.AttackAttribute ?? Define.AttackAttribute.None;
            var damageData = new DamageData(damage, attackAttribute);
            await t.AilmentController.OnTakeDamageAsync(t, a, damageData, cancellationToken);
            if (giveDamageType == Define.GiveDamageType.Direct && !t.BattleStatus.IsDead)
            {
                await a.BeginComboAsync(target, cancellationToken);
            }
        }
    }
}
