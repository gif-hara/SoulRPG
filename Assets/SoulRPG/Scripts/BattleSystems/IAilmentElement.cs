using Cysharp.Threading.Tasks;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAilmentElement
    {
        UniTask OnTurnEndAsync(BattleCharacter battleCharacter);
        
        bool IsEnd();
    }
}
