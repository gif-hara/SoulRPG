using R3;
using SoulRPG;
using UnityEngine.InputSystem;

namespace HK
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        public static MasterData.DungeonEvent GetMasterDataDungeonEvent(this string self)
        {
            return TinyServiceLocator.Resolve<MasterData>().DungeonEvents.Get(self);
        }
    }
}