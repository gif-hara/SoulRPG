using System.Threading;
using Cysharp.Threading.Tasks;

namespace SoulRPG.BattleSystems.CommandInvokers
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICommandInvoker
    {
        UniTask InvokeAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope);
    }
}
