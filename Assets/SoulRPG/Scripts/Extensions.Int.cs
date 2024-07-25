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
    }
}
