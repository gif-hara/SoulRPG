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
        private readonly int turnCount;

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
            return PlaySequencesAsync(masterDataAilment.Sequences.OnAdded, scope);
        }

        public UniTask OnRemovedAsync(BattleCharacter battleCharacter, CancellationToken scope)
        {
            return PlaySequencesAsync(masterDataAilment.Sequences.OnRemoved, scope);
        }

        public UniTask OnTurnEndAsync(BattleCharacter battleCharacter, CancellationToken scope)
        {
            currentTurnCount++;
            return PlaySequencesAsync(masterDataAilment.Sequences.OnTurnEnd, scope);
        }

        public bool IsEnd()
        {
            return currentTurnCount >= turnCount;
        }

        public int GetMasterDataId()
        {
            return masterDataAilmentId;
        }

        private static async UniTask<Container> PlaySequencesAsync(ScriptableSequences sequences, CancellationToken scope)
        {
            if (sequences == null)
            {
                return null;
            }

            var container = new Container();
            var sequencer = new Sequencer(container, sequences.Sequences);
            await sequencer.PlayAsync(scope);
            return container;
        }
    }
}
