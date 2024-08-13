using System;
using Cysharp.Threading.Tasks;
using SoulRPG.BattleSystems.CommandInvokers;
using UnitySequencerSystem;

namespace SoulRPG.BattleSystems.AI
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class Enemy : IBattleAI
    {
        private ScriptableSequences scriptableSequences;

        public Enemy(ScriptableSequences scriptableSequences)
        {
            this.scriptableSequences = scriptableSequences;
        }

        public void Dispose()
        {
        }

        public UniTask<ICommandInvoker> ThinkAsync(BattleCharacter character)
        {
            var result = new Sequences(scriptableSequences, 101999, 0, 1, 0);
            return UniTask.FromResult(result as ICommandInvoker);
        }
    }
}
