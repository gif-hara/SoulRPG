using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class BattleCharacterSequences
    {
        [SerializeField]
        private ScriptableSequences onTakeDamage;

        public UniTask PlayOnTakeDamageAsync(CancellationToken scope)
        {
            return new Sequencer(new Container(), onTakeDamage.Sequences).PlayAsync(scope);
        }
    }
}
