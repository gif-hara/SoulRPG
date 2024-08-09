using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public interface IExplorationView
    {
        UniTask OnOpenDoorAsync(DungeonInstanceWallData wallData);
    }
}
