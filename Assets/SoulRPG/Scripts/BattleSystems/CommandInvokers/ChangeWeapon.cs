using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using UnitySequencerSystem;

namespace SoulRPG.BattleSystems.CommandInvokers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ChangeWeapon : ICommandInvoker
    {
        private readonly int equipmentWeaponInventoryIndex;

        private readonly int changeWeaponId;

        public ChangeWeapon(int equipmentWeaponInventoryIndex, int changeWeaponId)
        {
            this.equipmentWeaponInventoryIndex = equipmentWeaponInventoryIndex;
            this.changeWeaponId = changeWeaponId;
        }

        public async UniTask InvokeAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope)
        {
            actor.Equipment.EquipWeapon(equipmentWeaponInventoryIndex, changeWeaponId);
            await TinyServiceLocator.Resolve<GameEvents>().ShowMessageAndWaitForSubmitInputAsync(new("装備している武器を変更した。", "Sfx.Message.24"));
        }

        public int GetNeedBehaviourPoint()
        {
            return 1;
        }

        public string GetIdentifier()
        {
            return "";
        }

        public bool CanRegisterUsedIdentifier()
        {
            return false;
        }

        public int GetNeedStamina()
        {
            return 0;
        }
    }
}
