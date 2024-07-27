using SoulRPG.CharacterControllers;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class BattleCharacter
    {
        public CharacterBattleStatus BattleStatus { get; }

        public BattleCharacter(Character character)
        {
            BattleStatus = new CharacterBattleStatus(character);
        }

        public BattleCharacter(CharacterBattleStatus battleStatus)
        {
            BattleStatus = battleStatus;
        }
    }
}
