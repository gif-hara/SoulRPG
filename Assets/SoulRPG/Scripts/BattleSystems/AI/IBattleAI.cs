using Cysharp.Threading.Tasks;
using SoulRPG.BattleSystems.CommandInvokers;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBattleAI
    {
        UniTask<ICommandInvoker> ThinkAsync(BattleCharacter character);
    }
}
