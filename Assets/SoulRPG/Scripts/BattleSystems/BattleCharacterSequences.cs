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
    [CreateAssetMenu(menuName = "SoulRPG/BattleCharacterSequences")]
    public sealed class BattleCharacterSequences : ScriptableObject
    {
        [SerializeField]
        private ScriptableSequences onTakeDamage;

        [SerializeField]
        private ScriptableSequences onDeadMessage;

        [SerializeField]
        private ScriptableSequences onBeginTurn;

        public UniTask PlayOnTakeDamageAsync(CancellationToken scope)
        {
            return new Sequencer(new Container(), onTakeDamage.Sequences).PlayAsync(scope);
        }

        public UniTask PlayOnDeadMessageAsync(BattleCharacter actor, CancellationToken scope)
        {
            var container = new Container();
            container.Register("Actor", actor);
            return new Sequencer(container, onDeadMessage.Sequences).PlayAsync(scope);
        }

        public UniTask PlayOnBeginTurnAsync(BattleCharacter actor, CancellationToken scope)
        {
            var container = new Container();
            container.Register("Actor", actor);
            return new Sequencer(container, onBeginTurn.Sequences).PlayAsync(scope);
        }
    }
}
