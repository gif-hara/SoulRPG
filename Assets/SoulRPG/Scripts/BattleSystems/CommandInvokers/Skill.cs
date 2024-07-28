using System.Threading;
using Cysharp.Threading.Tasks;
using UnitySequencerSystem;

namespace SoulRPG.BattleSystems.CommandInvokers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Skill : ICommandInvoker
    {
        private readonly int weaponId;

        private readonly int skillId;

        public Skill(int weaponId, int skillId)
        {
            this.weaponId = weaponId;
            this.skillId = skillId;
        }

        public UniTask InvokeAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope)
        {
            var masterDataSkill = skillId.GetMasterDataSkill();
            var sequences = masterDataSkill.ActionSequences.Sequences;
            var container = new Container();
            container.Register("Actor", actor);
            container.Register("Target", target);
            container.Register(weaponId.GetMasterDataWeapon());
            var sequencer = new Sequencer(container, sequences);
            return sequencer.PlayAsync(scope);
        }
    }
}