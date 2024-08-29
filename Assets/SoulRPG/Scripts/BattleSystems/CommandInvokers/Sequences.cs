using System.Threading;
using Cysharp.Threading.Tasks;
using UnitySequencerSystem;

namespace SoulRPG.BattleSystems.CommandInvokers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Sequences : ICommandInvoker
    {
        private readonly ScriptableSequences scriptableSequences;

        private readonly int weaponId;

        private readonly int behaviourPriority;

        private readonly int needBehaviourPoint;

        private readonly int needStamina;

        private readonly string identifier;

        private readonly bool canRegisterUsedIdentifier;

        public Sequences(ScriptableSequences scriptableSequences, int weaponId, int behaviourPriority, int needBehaviourPoint, int needStamina, string identifier)
            : this(scriptableSequences, weaponId, behaviourPriority, needBehaviourPoint, needStamina, identifier, true)
        {
        }

        public Sequences(ScriptableSequences scriptableSequences, int weaponId, int behaviourPriority, int needBehaviourPoint, int needStamina)
            : this(scriptableSequences, weaponId, behaviourPriority, needBehaviourPoint, needStamina, "", false)
        {
        }

        private Sequences(ScriptableSequences scriptableSequences, int weaponId, int behaviourPriority, int needBehaviourPoint, int needStamina, string identifier, bool canRegisterUsedIdentifier)
        {
            this.scriptableSequences = scriptableSequences;
            this.weaponId = weaponId;
            this.behaviourPriority = behaviourPriority;
            this.needBehaviourPoint = needBehaviourPoint;
            this.needStamina = needStamina;
            this.identifier = identifier;
            this.canRegisterUsedIdentifier = canRegisterUsedIdentifier;
        }

        public UniTask InvokeAsync(BattleCharacter actor, BattleCharacter target, Container container, CancellationToken scope)
        {
            var sequences = scriptableSequences.Sequences;
            container.Register("Actor", actor);
            container.Register("Target", target);
            container.Register(weaponId.GetMasterDataWeapon());
            var sequencer = new Sequencer(container, sequences);
            return sequencer.PlayAsync(scope);
        }

        public int GetNeedBehaviourPoint()
        {
            return needBehaviourPoint;
        }

        public string GetIdentifier()
        {
            return identifier;
        }

        public bool CanRegisterUsedIdentifier()
        {
            return canRegisterUsedIdentifier;
        }

        public int GetNeedStamina()
        {
            return needStamina;
        }
    }
}
