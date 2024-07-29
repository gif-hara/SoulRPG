using Cysharp.Threading.Tasks;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class AilmentElement : IAilmentElement
    {
        private readonly int turnCount;
        
        private int currentTurnCount;

        protected AilmentElement(int turnCount)
        {
            this.turnCount = turnCount;
        }
        
        public virtual UniTask OnTurnEndAsync(BattleCharacter battleCharacter)
        {
            currentTurnCount++;
            return UniTask.CompletedTask;
        }

        public bool IsEnd()
        {
            return currentTurnCount >= turnCount;
        }
    }
}
