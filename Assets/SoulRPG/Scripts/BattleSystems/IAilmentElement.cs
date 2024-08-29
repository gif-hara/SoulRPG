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

        UniTask OnTurnEndAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope);

        UniTask<bool> CanExecutableTurnAsync(BattleCharacter battleCharacter, CancellationToken scope);

        UniTask OnComboFromGivedDamageAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope);

        UniTask OnComboFromTakedDamageAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope);

        UniTask<bool> CanAddAilmentAsync(BattleCharacter battleCharacter, int pendingAilmentId, CancellationToken scope);

        UniTask<bool> EvaluateEvadeAsync(BattleCharacter battleCharacter, CancellationToken scope);

        UniTask OnResetAsync(BattleCharacter battleCharacter, int newTurnCount, CancellationToken scope);

        UniTask OnTakeDamageAsync(BattleCharacter actor, BattleCharacter target, DamageData damageData, CancellationToken scope);

        UniTask<int> OnCalculateNeedBehaviourPointAsync(BattleCharacter battleCharacter, int cost, CancellationToken scope);

        UniTask OnBehaviourEndAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope);

        UniTask OnTurnStartAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope);

        UniTask<bool> CanExecutableTurnOpponentAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope);

        UniTask<int> OnCalculateAilmentTurnCountAsync(BattleCharacter battleCharacter, int turnCount, CancellationToken scope);

        UniTask<int> OnCalculateAddExperienceAsync(BattleCharacter battleCharacter, float experience, CancellationToken scope);

        bool IsEnd();

        int GetMasterDataId();

        int GetRemainingTurnCount();

        int GetBehaviourCount();

        int GetInvokeCountThisTurn();

        void AddInvokeCountThisTurn(int value);

        MasterData.Ailment GetMasterDataAilment();
    }
}
