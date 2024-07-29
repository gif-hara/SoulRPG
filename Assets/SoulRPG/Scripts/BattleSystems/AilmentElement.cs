using System.Threading;
using Cysharp.Threading.Tasks;
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
            return PlaySequencesAsync(masterDataAilment.Sequences.OnAdded, battleCharacter, scope);
        }

        public UniTask OnRemovedAsync(BattleCharacter battleCharacter, CancellationToken scope)
        {
            return PlaySequencesAsync(masterDataAilment.Sequences.OnRemoved, battleCharacter, scope);
        }

        public UniTask OnTurnEndAsync(BattleCharacter battleCharacter, CancellationToken scope)
        {
            currentTurnCount++;
            return PlaySequencesAsync(masterDataAilment.Sequences.OnTurnEnd, battleCharacter, scope);
        }

        public bool IsEnd()
        {
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

        private static async UniTask<Container> PlaySequencesAsync(ScriptableSequences sequences, BattleCharacter battleCharacter, CancellationToken scope)
        {
            if (sequences == null)
            {
                return null;
            }

            var container = new Container();
            container.Register("Actor", battleCharacter);
            var sequencer = new Sequencer(container, sequences.Sequences);
            await sequencer.PlayAsync(scope);
            return container;
        }
    }
}
