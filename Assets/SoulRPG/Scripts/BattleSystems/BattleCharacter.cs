using Cysharp.Threading.Tasks;
using SoulRPG.BattleSystems.CommandInvokers;
using SoulRPG.CharacterControllers;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class BattleCharacter
    {
        public CharacterBattleStatus BattleStatus { get; }

        public Equipment Equipment { get; }

        private readonly IBattleAI battleAI;

        public BattleCharacter(Character character, IBattleAI battleAI)
        {
            BattleStatus = new CharacterBattleStatus(character);
            Equipment = character.Equipment;
            this.battleAI = battleAI;
        }

        public BattleCharacter(CharacterBattleStatus battleStatus, IBattleAI battleAI)
        {
            BattleStatus = battleStatus;
            this.battleAI = battleAI;
        }

        public UniTask<ICommandInvoker> ThinkAsync()
        {
            return battleAI.ThinkAsync(this);
        }
    }
}
