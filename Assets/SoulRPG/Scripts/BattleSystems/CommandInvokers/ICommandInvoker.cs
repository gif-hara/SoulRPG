using System.Threading;
using Cysharp.Threading.Tasks;
using UnitySequencerSystem;

namespace SoulRPG.BattleSystems.CommandInvokers
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICommandInvoker
    {
        UniTask InvokeAsync(BattleCharacter actor, BattleCharacter target, Container container, CancellationToken scope);

        int GetNeedBehaviourPoint();

        int GetNeedStamina();

        string GetIdentifier();

        bool CanRegisterUsedIdentifier();
    }
}
