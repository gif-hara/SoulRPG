using System;
using Cysharp.Threading.Tasks;
using SoulRPG.BattleSystems.CommandInvokers;
using SoulRPG.CharacterControllers;
using UnityEngine;

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

        public void Dispose()
        {
            BattleStatus.Dispose();
        }
    }
}
