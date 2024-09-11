using Cysharp.Threading.Tasks;
using HK;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public interface IExplorationView
    {
        UniTask OnOpenDoorAsync(DungeonInstanceWallData wallData);
        
        HKUIDocument GetFloorEventDocument(DungeonInstanceFloorData data);
    }
}
