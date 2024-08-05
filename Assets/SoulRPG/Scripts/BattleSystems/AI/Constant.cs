using System;
using Cysharp.Threading.Tasks;
using SoulRPG.BattleSystems.CommandInvokers;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class Constant : IBattleAI
    {
        [SerializeField]
        private int weaponItemId;

        [SerializeField]
        private int skillId;

        [SerializeField]
        private bool canRegisterUsedIdentifier;

        public Constant(int weaponItemId, int skillId, bool canRegisterUsedIdentifier)
        {
            this.weaponItemId = weaponItemId;
            this.skillId = skillId;
            this.canRegisterUsedIdentifier = canRegisterUsedIdentifier;
        }

        public UniTask<ICommandInvoker> ThinkAsync(BattleCharacter character)
        {
            return UniTask.FromResult(new Skill(weaponItemId, skillId, canRegisterUsedIdentifier) as ICommandInvoker);
        }
    }
}
