using System;
using Cysharp.Threading.Tasks;
using SoulRPG.BattleSystems.CommandInvokers;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBattleAI : IDisposable
    {
        UniTask<ICommandInvoker> ThinkAsync(BattleCharacter actor, BattleCharacter target);
    }
}
