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
    public sealed class RemoveStrengthBuff : ISequence
    {
        [SerializeField]
        private Define.AttackType attackType;

        [SerializeField]
        private string buffName;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var actor = container.Resolve<BattleCharacter>("Actor");
            actor.StrengthBuffController.Remove(attackType, buffName);
            return UniTask.CompletedTask;
        }
    }
}
