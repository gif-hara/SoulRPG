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

        public Sequences(ScriptableSequences scriptableSequences, int weaponId, int behaviourPriority, int cost)
        {
            this.scriptableSequences = scriptableSequences;
            this.weaponId = weaponId;
            this.behaviourPriority = behaviourPriority;
            this.cost = cost;
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

        public int GetSpeed()
        {
            return weaponId.GetMasterDataWeapon().Speed;
        }

        public int GetBehaviourPriority()
        {
            return behaviourPriority;
        }

        public int GetCost()
        {
            return cost;
        }
    }
}
