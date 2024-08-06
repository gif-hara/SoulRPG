using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Assertions;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class AilmentElement : IAilmentElement
    {
        private int turnCount;

        private readonly MasterData.Ailment masterDataAilment;

        private readonly int masterDataAilmentId;

        private int currentTurnCount;

        public AilmentElement(int masterDataAilmentId, int turnCount)
        {
            masterDataAilment = masterDataAilmentId.GetMasterDataAilment();
            this.masterDataAilmentId = masterDataAilmentId;
            this.turnCount = turnCount;
        }

        public UniTask OnAddedAsync(BattleCharacter battleCharacter, CancellationToken scope)
        {
            Assert.IsNotNull(battleCharacter, "battleCharacterがnullです");
            Assert.IsNotNull(masterDataAilment, $"マスターデータが存在しません masterDataAilmentId: {masterDataAilmentId}");
            return PlaySequencesAsync(masterDataAilment.Sequences.GetSequences(Define.AilmentBehaviourType.OnAdded), battleCharacter, null, scope);
        }

        public UniTask OnRemovedAsync(BattleCharacter battleCharacter, CancellationToken scope)
        {
            return PlaySequencesAsync(masterDataAilment.Sequences.GetSequences(Define.AilmentBehaviourType.OnRemoved), battleCharacter, null, scope);
        }

        public UniTask OnTurnEndAsync(BattleCharacter battleCharacter, CancellationToken scope)
        {
            currentTurnCount++;
            return PlaySequencesAsync(masterDataAilment.Sequences.GetSequences(Define.AilmentBehaviourType.OnTurnEnd), battleCharacter, null, scope);
        }

        public async UniTask<bool> CanExecutableTurnAsync(BattleCharacter battleCharacter, CancellationToken scope)
        {
            var container = await PlaySequencesAsync(masterDataAilment.Sequences.GetSequences(Define.AilmentBehaviourType.CanExecutableTurn), battleCharacter, null, scope);
            if (container == null)
            {
                return true;
            }

            var contains = container.TryResolve<bool>("CanExecutableTurn", out var canExecutableTurn);
            return contains && canExecutableTurn;
        }

        public UniTask OnComboFromGivedDamageAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope)
        {
            return PlaySequencesAsync(
                masterDataAilment.Sequences.GetSequences(Define.AilmentBehaviourType.OnComboFromGivedDamage),
                actor,
                c => c.Register("Target", target),
                scope
                );
        }

        public UniTask OnComboFromTakedDamageAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope)
        {
            return PlaySequencesAsync(
                masterDataAilment.Sequences.GetSequences(Define.AilmentBehaviourType.OnComboFromTakedDamage),
                actor,
                c => c.Register("Target", target),
                scope
                );
        }

        public async UniTask<bool> CanAddAilmentAsync(BattleCharacter battleCharacter, int pendingAilmentId, CancellationToken scope)
        {
            var container = await PlaySequencesAsync(
                masterDataAilment.Sequences.GetSequences(Define.AilmentBehaviourType.CanAddAilment),
                battleCharacter,
                c => c.Register("PendingAilmentId", pendingAilmentId),
                scope
                );
            if (container == null)
            {
                return true;
            }

            var contains = container.TryResolve<bool>("CanAddAilment", out var canAddAilment);
            return contains && canAddAilment;
        }

        public bool IsEnd()
        {
            if (turnCount == -1)
            {
                return false;
            }
            return currentTurnCount >= turnCount;
        }

        public int GetMasterDataId()
        {
            return masterDataAilmentId;
        }

        public void Reset(int turnCount)
        {
            // 無限付与の場合はリセットしない
            if (this.turnCount == -1)
            {
                return;
            }
            currentTurnCount = 0;
            this.turnCount = turnCount;
        }

        private static async UniTask<Container> PlaySequencesAsync(ScriptableSequences sequences, BattleCharacter battleCharacter, Action<Container> containerAction, CancellationToken scope)
        {
            if (sequences == null)
            {
                return null;
            }

            var container = new Container();
            container.Register("Actor", battleCharacter);
            containerAction?.Invoke(container);
            var sequencer = new Sequencer(container, sequences.Sequences);
            await sequencer.PlayAsync(scope);
            return container;
        }

        public int GetRemainingTurnCount()
        {
            return turnCount - currentTurnCount;
        }

        public MasterData.Ailment GetMasterDataAilment()
        {
            return masterDataAilment;
        }
    }
}
