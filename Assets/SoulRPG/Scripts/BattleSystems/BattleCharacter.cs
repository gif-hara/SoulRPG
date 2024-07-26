using SoulRPG.CharacterControllers;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class BattleCharacter
    {
        private readonly Character character;

        public CharacterBattleStatus BattleStatus { get; }
        
        public BattleCharacter(Character character)
        {
            this.character = character;
            BattleStatus = new CharacterBattleStatus(character);
        }
    }
}
