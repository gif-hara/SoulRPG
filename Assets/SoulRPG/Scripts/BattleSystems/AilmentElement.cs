namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class AilmentElement : IAilmentElement
    {
        private readonly int turnCount;
        
        private int currentTurnCount;
        
        public AilmentElement(int turnCount)
        {
            this.turnCount = turnCount;
        }
        
        public void OnTurnEnd(BattleCharacter battleCharacter)
        {
            currentTurnCount++;
        }

        public bool IsEnd()
        {
            return currentTurnCount >= turnCount;
        }
    }
}
