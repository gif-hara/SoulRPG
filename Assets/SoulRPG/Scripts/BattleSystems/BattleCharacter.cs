using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoulRPG.BattleSystems.CommandInvokers;
using SoulRPG.CharacterControllers;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class BattleCharacter : IDisposable
    {
        public CharacterBattleStatus BattleStatus { get; }

        public Equipment Equipment { get; }

        private readonly IBattleAI battleAI;

        public ICommandInvoker AfterCommandInvoker { get; set; }

        public AilmentController AilmentController { get; }

        public StatusBuffController StrengthBuffController { get; } = new();

        public BattleCharacter(Character character, IBattleAI battleAI)
        {
            BattleStatus = new CharacterBattleStatus(character);
            Equipment = character.Equipment;
            this.battleAI = battleAI;
            AilmentController = new AilmentController(this);
        }

        public BattleCharacter(CharacterBattleStatus battleStatus, IBattleAI battleAI)
        {
            BattleStatus = battleStatus;
            this.battleAI = battleAI;
            AilmentController = new AilmentController(this);
        }

        public async UniTask<ICommandInvoker> ThinkAsync()
        {
            if (!await AilmentController.CanExecutableTurnAsync())
            {
                return null;
            }

            return await battleAI.ThinkAsync(this);
        }

        public UniTask TurnEndAsync()
        {
            return AilmentController.OnTurnEndAsync();
        }

        public async UniTask BeginComboAsync(BattleCharacter target, CancellationToken scope)
        {
            await AilmentController.OnComboFromGivedDamageAsync(this, target, scope);
            await target.AilmentController.OnComboFromTakedDamageAsync(target, this, scope);
        }

        public void Dispose()
        {
            BattleStatus.Dispose();
            AilmentController.Dispose();
        }
    }
}
