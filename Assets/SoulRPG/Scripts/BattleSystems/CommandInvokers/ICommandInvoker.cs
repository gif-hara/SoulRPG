using Cysharp.Threading.Tasks;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICommandInvoker
    {
        UniTask InvokeAsync(BattleCharacter actor, BattleCharacter target);
    }
}
