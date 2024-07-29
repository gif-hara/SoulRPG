namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAilmentElement
    {
        void OnTurnEnd(BattleCharacter battleCharacter);
        
        bool IsEnd();
    }
}
