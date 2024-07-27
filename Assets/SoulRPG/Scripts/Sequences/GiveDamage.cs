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
        private bool waitForInput = true;

        public async UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var target = container.Resolve<BattleCharacter>("Target");
            if (target.BattleStatus.IsDead)
            {
                return;
            }
            var actor = container.Resolve<BattleCharacter>("Actor");
            var weapon = container.Resolve<MasterData.Weapon>();
            var damage = damageCalculator.Calculate(actor, target, weapon);
            target.BattleStatus.TakeDamage(damage);
            var message = $"{target.BattleStatus.Name}に{damage}のダメージ！";
            TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext(message);
            if (waitForInput)
            {
                await TinyServiceLocator.Resolve<GameEvents>().OnSubmitInput.FirstAsync();
            }
        }
    }
}
