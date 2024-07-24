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
    }
}
