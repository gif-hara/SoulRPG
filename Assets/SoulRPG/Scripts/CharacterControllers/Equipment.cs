using System.Collections.Generic;
using R3;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Equipment
    {
        private readonly List<ReactiveProperty<int>> weaponIds = new();
        public ReadOnlyReactiveProperty<int> WeaponIdAsObservable(int index) => weaponIds[index];

        private readonly ReactiveProperty<int> headId = new();
        public ReadOnlyReactiveProperty<int> HeadIdAsObservable() => headId;

        private readonly ReactiveProperty<int> bodyId = new();
        public ReadOnlyReactiveProperty<int> BodyIdAsObservable() => bodyId;

        private readonly ReactiveProperty<int> armId = new();
        public ReadOnlyReactiveProperty<int> ArmIdAsObservable() => armId;

        private readonly ReactiveProperty<int> legId = new();
        public ReadOnlyReactiveProperty<int> LegIdAsObservable() => legId;

        private readonly List<ReactiveProperty<int>> accessoryIds = new();
        public ReadOnlyReactiveProperty<int> AccessoryIdAsObservable(int index) => accessoryIds[index];

        public Equipment()
        {
            for (var i = 0; i < 3; i++)
            {
                weaponIds.Add(new ReactiveProperty<int>());
            }

            for (var i = 0; i < 4; i++)
            {
                accessoryIds.Add(new ReactiveProperty<int>());
            }
        }

        public void EquipWeapon(int index, int weaponId)
        {
            weaponIds[index].Value = weaponId;
        }

        public void EquipHead(int headId)
        {
            this.headId.Value = headId;
        }

        public void EquipBody(int bodyId)
        {
            this.bodyId.Value = bodyId;
        }

        public void EquipArm(int armId)
        {
            this.armId.Value = armId;
        }

        public void EquipLeg(int legId)
        {
            this.legId.Value = legId;
        }

        public void EquipAccessory(int index, int accessoryId)
        {
            accessoryIds[index].Value = accessoryId;
        }
    }
}
