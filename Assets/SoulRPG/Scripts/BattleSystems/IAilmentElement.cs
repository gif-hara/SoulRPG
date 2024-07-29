using System.Threading;
using Cysharp.Threading.Tasks;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAilmentElement
    {
        UniTask OnAddedAsync(BattleCharacter battleCharacter, CancellationToken scope);

        UniTask OnRemovedAsync(BattleCharacter battleCharacter, CancellationToken scope);

        UniTask OnTurnEndAsync(BattleCharacter battleCharacter, CancellationToken scope);

        UniTask<bool> CanExecutableTurnAsync(BattleCharacter battleCharacter, CancellationToken scope);

        UniTask OnGivedDamageAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope);

        bool IsEnd();

        int GetMasterDataId();

        void Reset(int turnCount);
    }
}
