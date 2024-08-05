using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoulRPG.BattleSystems.CommandInvokers;
using SoulRPG.CharacterControllers;
using UnityEngine.Assertions;

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

        public Queue<(string, ICommandInvoker)> AfterCommandInvokers { get; private set; } = new();

        public AilmentController AilmentController { get; }

        public StatusBuffController StatusBuffController { get; } = new();

        public HashSet<string> UsedSkills { get; } = new();

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

            var result = await battleAI.ThinkAsync(this);
            Assert.IsNotNull(result);
            if (!result.CanRegisterUsedIdentifier())
            {
                return result;
            }
            else
            {
                var identifier = result.GetIdentifier();
                Assert.IsFalse(UsedSkills.Contains(identifier), $"{identifier} is already used");
                UsedSkills.Add(identifier);
                return result;
            }
        }

        public UniTask TurnStartAsync()
        {
            UsedSkills.Clear();
            BattleStatus.RecoveryBehaviourPoint();
            return UniTask.CompletedTask;
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

        public float GetTotalCutRate(Define.AttackAttribute attackAttribute)
        {
            return BattleStatus.GetCutRate(attackAttribute) + StatusBuffController.GetCutRate(attackAttribute);
        }

        public bool ContainsAfterCommandInvoker(string key)
        {
            foreach (var (k, _) in AfterCommandInvokers)
            {
                if (k == key)
                {
                    return true;
                }
            }

            return false;
        }

        public void EnqueueAfterCommandInvoker(string key, ICommandInvoker commandInvoker)
        {
            AfterCommandInvokers.Enqueue((key, commandInvoker));
        }

        public async UniTask InvokeAfterCommandAsync(BattleCharacter target, CancellationToken scope)
        {
            var invokers = new List<(string, ICommandInvoker)>(AfterCommandInvokers);
            AfterCommandInvokers.Clear();
            foreach (var (_, invoker) in invokers)
            {
                await invoker.InvokeAsync(this, target, scope);
            }
        }
    }
}
