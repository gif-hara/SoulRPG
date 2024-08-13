using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoulRPG.BattleSystems.AI;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class ChangeEnemyAI : ISequence
    {
        [SerializeField]
        private ScriptableSequences scriptableSequences;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var actor = container.Resolve<BattleCharacter>("Actor");
            actor.ChangeAI(new Enemy(scriptableSequences));
            return UniTask.CompletedTask;
        }
    }
}
