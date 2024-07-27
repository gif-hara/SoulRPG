using System;
using Cysharp.Threading.Tasks;
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

        public Constant(int weaponItemId, int skillId)
        {
            this.weaponItemId = weaponItemId;
            this.skillId = skillId;
        }

        public UniTask<(int weaponItemId, int skillId)> ThinkAsync(BattleCharacter character)
        {
            return UniTask.FromResult((weaponItemId, skillId));
        }
    }
}
