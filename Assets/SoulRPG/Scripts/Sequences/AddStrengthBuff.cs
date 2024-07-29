using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoulRPG.BattleSystems.BattleCharacterEvaluators;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class AddStrengthBuff : ISequence
    {
        [SerializeField]
        private Define.AttackType attackType;

        [SerializeField]
        private string buffName;

        [SerializeField]
        private float rate;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var actor = container.Resolve<BattleCharacter>("Actor");
            actor.StrengthBuffController.Add(attackType, buffName, rate);
            return UniTask.CompletedTask;
        }
    }
}
