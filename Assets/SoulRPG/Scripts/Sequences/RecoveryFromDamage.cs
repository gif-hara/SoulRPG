using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class RecoveryFromDamage : ISequence
    {
        [SerializeField]
        private Define.TargetType targetType;

        [SerializeField]
        private float rate;

        [SerializeField]
        private string sfxName = "Sfx.Message.19";

        public async UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var t = targetType == Define.TargetType.Self
                ? container.Resolve<BattleCharacter>("Actor")
                : container.Resolve<BattleCharacter>("Target");
            var damage = container.Resolve<int>("Damage");
            var recoveryPoint = Mathf.FloorToInt(damage * rate);
            t.BattleStatus.RecoveryHitPoint(recoveryPoint);
            await TinyServiceLocator.Resolve<GameEvents>()
                .ShowMessageAndWaitForSubmitInputAsync(new($"{t.BattleStatus.NameWithTag}は<color=#88FF88>{recoveryPoint}</color>回復した。", sfxName));
        }
    }
}
