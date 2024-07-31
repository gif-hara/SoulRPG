using Cysharp.Threading.Tasks;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public interface IExplorationView
    {
        UniTask OnOpenDoorAsync(MasterData.WallEvent wallEvent);
    }
}
