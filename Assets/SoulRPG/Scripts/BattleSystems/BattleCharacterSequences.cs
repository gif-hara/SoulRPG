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

        [SerializeField]
        private ScriptableSequences onBeginBattle;

        [SerializeField]
        private ScriptableSequences onAppearance;

        public UniTask PlayOnTakeDamageAsync(CancellationToken scope)
        {
            return new Sequencer(new Container(), onTakeDamage.Sequences).PlayAsync(scope);
        }

        public UniTask PlayOnDeadMessageAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope)
        {
            var container = new Container();
            container.Register("Actor", actor);
            container.Register("Target", target);
            return new Sequencer(container, onDeadMessage.Sequences).PlayAsync(scope);
        }

        public UniTask PlayOnBeginTurnAsync(BattleCharacter actor, CancellationToken scope)
        {
            var container = new Container();
            container.Register("Actor", actor);
            return new Sequencer(container, onBeginTurn.Sequences).PlayAsync(scope);
        }
        
        public UniTask PlayOnBeginBattleAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope)
        {
            var container = new Container();
            container.Register("Actor", actor);
            container.Register("Target", target);
            return new Sequencer(container, onBeginBattle.Sequences).PlayAsync(scope);
        }
        
        public UniTask PlayOnAppearanceAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope)
        {
            var container = new Container();
            container.Register("Actor", actor);
            container.Register("Target", target);
            return new Sequencer(container, onAppearance.Sequences).PlayAsync(scope);
        }
    }
}
