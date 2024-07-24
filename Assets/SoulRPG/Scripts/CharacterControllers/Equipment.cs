using System.Collections.Generic;
using System.Linq;
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
        public int GetWeaponId(int index) => weaponIds[index].Value;
        public IEnumerable<int> GetWeaponIds() => weaponIds.Select(x => x.Value);

        private readonly ReactiveProperty<int> headId = new();
        public ReadOnlyReactiveProperty<int> HeadIdAsObservable() => headId;
        public int HeadId => headId.Value;

        private readonly ReactiveProperty<int> bodyId = new();
        public ReadOnlyReactiveProperty<int> BodyIdAsObservable() => bodyId;
        public int BodyId => bodyId.Value;

        private readonly ReactiveProperty<int> armId = new();
        public ReadOnlyReactiveProperty<int> ArmIdAsObservable() => armId;
        public int ArmId => armId.Value;

        private readonly ReactiveProperty<int> legId = new();
        public ReadOnlyReactiveProperty<int> LegIdAsObservable() => legId;
        public int LegId => legId.Value;

        private readonly List<ReactiveProperty<int>> accessoryIds = new();
        public ReadOnlyReactiveProperty<int> AccessoryIdAsObservable(int index) => accessoryIds[index];
        public int GetAccessoryId(int index) => accessoryIds[index].Value;
        public IEnumerable<int> GetAccessoryIds() => accessoryIds.Select(x => x.Value);

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
            foreach (var i in weaponIds)
            {
                // 既に装備している場合は入れ替える
                if (i.Value == weaponId)
                {
                    i.Value = weaponIds[index].Value;
                    break;
                }
            }
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
