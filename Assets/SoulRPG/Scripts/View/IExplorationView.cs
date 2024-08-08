using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public interface IExplorationView
    {
        UniTask OnOpenDoorAsync((Vector2Int from, Vector2Int to) positions);
    }
}
