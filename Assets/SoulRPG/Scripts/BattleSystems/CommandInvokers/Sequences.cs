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

        private readonly int cost;

        private readonly string identifier;

        private readonly bool canRegisterUsedIdentifier;

        public Sequences(ScriptableSequences scriptableSequences, int weaponId, int behaviourPriority, int cost, string identifier)
            : this(scriptableSequences, weaponId, behaviourPriority, cost, identifier, true)
        {
        }

        public Sequences(ScriptableSequences scriptableSequences, int weaponId, int behaviourPriority, int cost)
            : this(scriptableSequences, weaponId, behaviourPriority, cost, "", false)
        {
        }

        private Sequences(ScriptableSequences scriptableSequences, int weaponId, int behaviourPriority, int cost, string identifier, bool canRegisterUsedIdentifier)
        {
            this.scriptableSequences = scriptableSequences;
            this.weaponId = weaponId;
            this.behaviourPriority = behaviourPriority;
            this.cost = cost;
            this.identifier = identifier;
            this.canRegisterUsedIdentifier = canRegisterUsedIdentifier;
        }

        public UniTask InvokeAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope)
        {
            var sequences = scriptableSequences.Sequences;
            var container = new Container();
            container.Register("Actor", actor);
            container.Register("Target", target);
            container.Register(weaponId.GetMasterDataWeapon());
            var sequencer = new Sequencer(container, sequences);
            return sequencer.PlayAsync(scope);
        }

        public int GetCost()
        {
            return cost;
        }

        public string GetIdentifier()
        {
            return identifier;
        }

        public bool CanRegisterUsedIdentifier()
        {
            return canRegisterUsedIdentifier;
        }
    }
}
