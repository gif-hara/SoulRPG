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

        UniTask OnComboFromGivedDamageAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope);

        UniTask OnComboFromTakedDamageAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope);

        UniTask<bool> CanAddAilmentAsync(BattleCharacter battleCharacter, int pendingAilmentId, CancellationToken scope);

        UniTask<bool> EvaluateEvadeAsync(BattleCharacter battleCharacter, CancellationToken scope);

        UniTask OnResetAsync(BattleCharacter battleCharacter, int newTurnCount, CancellationToken scope);

        UniTask OnTakeDamageAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope);

        UniTask<int> OnCalculateBehaviourPointAsync(BattleCharacter battleCharacter, int behaviourPoint, CancellationToken scope);

        UniTask<int> OnCalculateNeedBehaviourPointAsync(BattleCharacter battleCharacter, int cost, CancellationToken scope);

        bool IsEnd();

        int GetMasterDataId();

        int GetRemainingTurnCount();

        MasterData.Ailment GetMasterDataAilment();
    }
}
