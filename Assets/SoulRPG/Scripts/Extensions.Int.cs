using HK;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        public static MasterData.Item GetMasterDataItem(this int self)
        {
            return TinyServiceLocator.Resolve<MasterData>().Items.Get(self);
        }

        public static MasterData.Weapon GetMasterDataWeapon(this int self)
        {
            return TinyServiceLocator.Resolve<MasterData>().Weapons.Get(self);
        }

        public static bool TryGetMasterDataWeapon(this int self, out MasterData.Weapon weapon)
        {
            return TinyServiceLocator.Resolve<MasterData>().Weapons.TryGetValue(self, out weapon);
        }

        public static bool TryGetMasterDataArmorHead(this int self, out MasterData.Armor armorHead)
        {
            return TinyServiceLocator.Resolve<MasterData>().ArmorHeads.TryGetValue(self, out armorHead);
        }

        public static bool TryGetMasterDataArmorBody(this int self, out MasterData.Armor armorBody)
        {
            return TinyServiceLocator.Resolve<MasterData>().ArmorBodies.TryGetValue(self, out armorBody);
        }

        public static bool TryGetMasterDataArmorArms(this int self, out MasterData.Armor armorArms)
        {
            return TinyServiceLocator.Resolve<MasterData>().ArmorArms.TryGetValue(self, out armorArms);
        }

        public static bool TryGetMasterDataArmorLegs(this int self, out MasterData.Armor armorLegs)
        {
            return TinyServiceLocator.Resolve<MasterData>().ArmorLegs.TryGetValue(self, out armorLegs);
        }

        public static bool ContainsMasterDataWeapon(this int self)
        {
            return TinyServiceLocator.Resolve<MasterData>().Weapons.ContainsKey(self);
        }

        public static bool ContainsMasterDataArmorHead(this int self)
        {
            return TinyServiceLocator.Resolve<MasterData>().ArmorHeads.ContainsKey(self);
        }

        public static bool ContainsMasterDataArmorBody(this int self)
        {
            return TinyServiceLocator.Resolve<MasterData>().ArmorBodies.ContainsKey(self);
        }

        public static bool ContainsMasterDataArmorArms(this int self)
        {
            return TinyServiceLocator.Resolve<MasterData>().ArmorArms.ContainsKey(self);
        }

        public static bool ContainsMasterDataArmorLegs(this int self)
        {
            return TinyServiceLocator.Resolve<MasterData>().ArmorLegs.ContainsKey(self);
        }

        public static bool ContainsMasterDataAccessory(this int self)
        {
            return TinyServiceLocator.Resolve<MasterData>().Accessories.ContainsKey(self);
        }

        public static bool ContainsMasterDataSkill(this int self)
        {
            return TinyServiceLocator.Resolve<MasterData>().Skills.ContainsKey(self);
        }

        public static MasterData.Skill GetMasterDataSkill(this int self)
        {
            return TinyServiceLocator.Resolve<MasterData>().Skills.Get(self);
        }
        
        public static MasterData.Ailment GetMasterDataAilment(this int self)
        {
            return TinyServiceLocator.Resolve<MasterData>().Ailments.Get(self);
        }
    }
}
