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

        private readonly bool canRegisterUsedIdentifier;

        public Skill(int weaponId, int skillId, bool canRegisterUsedIdentifier)
        {
            this.weaponId = weaponId;
            this.skillId = skillId;
            this.canRegisterUsedIdentifier = canRegisterUsedIdentifier;
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

        public int GetNeedBehaviourPoint()
        {
            return skillId.GetMasterDataSkill().NeedBehaviourPoint;
        }

        public string GetIdentifier()
        {
            return CreateIdentifier(weaponId, skillId);
        }

        public bool CanRegisterUsedIdentifier()
        {
            return canRegisterUsedIdentifier;
        }

        public static string CreateIdentifier(int weaponId, int skillId)
        {
            return $"{weaponId}.{skillId}";
        }

        public int GetNeedStamina()
        {
            return skillId.GetMasterDataSkill().NeedStamina;
        }
    }
}
