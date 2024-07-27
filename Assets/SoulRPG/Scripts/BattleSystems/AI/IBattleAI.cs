using Cysharp.Threading.Tasks;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBattleAI
    {
        UniTask<(int weaponItemId, int skillId)> ThinkAsync(BattleCharacter character);
    }
}
