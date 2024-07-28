using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.BattleSystems.CommandInvokers;
using SoulRPG.BattleSystems.DamageCalculators;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class RegisterAfterCommandInvoker : ISequence
    {
        [SerializeField]
        private ScriptableSequences scriptableSequences;

        [SerializeField]
        private int behaviourPriority;


        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var target = container.Resolve<BattleCharacter>("Target");
            if (target.BattleStatus.IsDead)
            {
                return UniTask.CompletedTask;
            }
            var actor = container.Resolve<BattleCharacter>("Actor");
            var weapon = container.Resolve<MasterData.Weapon>();
            actor.AfterCommandInvoker = new Sequences(scriptableSequences, weapon.ItemId, behaviourPriority);
            return UniTask.CompletedTask;
        }
    }
}
