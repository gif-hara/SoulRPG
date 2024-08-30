using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using NUnit.Framework;
using SoulRPG.BattleSystems.CommandInvokers;
using SoulRPG.CharacterControllers;
using UnityEngine;
using UnityEngine.Assertions;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class BattleCharacter : IDisposable
    {
        public CharacterBattleStatus BattleStatus { get; }

        public Equipment Equipment { get; }

        private IBattleAI battleAI;

        public Queue<(string, ICommandInvoker)> AfterCommandInvokers { get; private set; } = new();

        public AilmentController AilmentController { get; }

        public StatusBuffController StatusBuffController { get; } = new();

        public HashSet<string> UsedSkills { get; } = new();

        public BattleCharacterEvents Events { get; } = new();

        private readonly BattleCharacterSequences sequences;

        private readonly CancellationTokenSource scope = new();

        public Character Character { get; }

        public bool IsDisposed { get; private set; }

        public BattleCharacter(Character character, Define.AllyType allyType, IBattleAI battleAI, BattleCharacterSequences sequences)
        {
            Character = character;
            BattleStatus = new CharacterBattleStatus(character, allyType);
            Equipment = character.Equipment;
            this.battleAI = battleAI;
            AilmentController = new AilmentController(this);
            this.sequences = sequences;
        }

        public BattleCharacter(CharacterBattleStatus battleStatus, IBattleAI battleAI, BattleCharacterSequences sequences)
        {
            BattleStatus = battleStatus;
            this.battleAI = battleAI;
            AilmentController = new AilmentController(this);
            this.sequences = sequences;
        }

        public async UniTask<ICommandInvoker> ThinkAsync(BattleCharacter target)
        {
            var result = await battleAI.ThinkAsync(this);
            UnityEngine.Assertions.Assert.IsNotNull(result);
            if (!result.CanRegisterUsedIdentifier())
            {
                return result;
            }
            else
            {
                var identifier = result.GetIdentifier();
                UnityEngine.Assertions.Assert.IsFalse(UsedSkills.Contains(identifier), $"{identifier} is already used");
                UsedSkills.Add(identifier);
                return result;
            }
        }

        public async UniTask<bool> TurnStartAsync(BattleCharacter target)
        {
            UsedSkills.Clear();
            if (!await AilmentController.CanExecutableTurnAsync())
            {
                return false;
            }

            if (!await target.AilmentController.CanExecutableTurnOpponentAsync(this))
            {
                return false;
            }
            BattleStatus.RecoveryBehaviourPoint();
            await AilmentController.OnTurnStartAsync(this, target, scope.Token);
            await sequences.PlayOnBeginTurnAsync(this, scope.Token);
            return true;
        }

        public UniTask TurnEndAsync(BattleCharacter target)
        {
            return AilmentController.OnTurnEndAsync(target);
        }

        public async UniTask BeginComboAsync(BattleCharacter target, CancellationToken scope)
        {
            await AilmentController.OnComboFromGivedDamageAsync(this, target, scope);
            await target.AilmentController.OnComboFromTakedDamageAsync(target, this, scope);
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }
            IsDisposed = true;
            BattleStatus.Dispose();
            AilmentController.Dispose();
            battleAI.Dispose();
            scope.Cancel();
            scope.Dispose();
        }

        public float GetTotalCutRate(Define.AttackAttribute attackAttribute, BattleCharacter target, Container container)
        {
            var battleStatusCutRate = BattleStatus.GetCutRate(attackAttribute);
            var statusBuffCutRate = StatusBuffController.GetCutRate(attackAttribute, this, target, container);
            Debug.Log($"attackAttribute: {attackAttribute}, battleStatusCutRate: {battleStatusCutRate}, statusBuffCutRate: {statusBuffCutRate}");
            return battleStatusCutRate + statusBuffCutRate;
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
                await invoker.InvokeAsync(this, target, new Container(), scope);
            }
        }

        public UniTask<bool> EvaluateEvaded()
        {
            return AilmentController.EvaluateEvadeAsync(this);
        }

        public async UniTask<int> GetFixedNeedBehaviourPointAsync(int cost)
        {
            var result = await AilmentController.OnCalculateNeedBehaviourPointAsync(cost);
            return Mathf.Max(0, result);
        }

        public async UniTask<int> GetFixedNeedStaminaAsync(int cost)
        {
            var result = await AilmentController.OnCalculateNeedStaminaAsync(cost);
            return Mathf.Max(0, result);
        }

        public UniTask OnBehaviourEndAsync(BattleCharacter target, CancellationToken scope)
        {
            return AilmentController.OnBehaviourEndAsync(this, target, scope);
        }

        public void ChangeAI(IBattleAI battleAI)
        {
            this.battleAI = battleAI;
        }

        public async UniTask TakeDamageAsync(int damage)
        {
            var canDamage = true;
#if DEBUG
            var battleDebugData = TinyServiceLocator.Resolve<BattleDebugData>();
            if (BattleStatus.AllyType == Define.AllyType.Player && battleDebugData.IsInvinciblePlayer)
            {
                canDamage = false;
            }
            if (BattleStatus.AllyType == Define.AllyType.Enemy && battleDebugData.IsInvincibleEnemy)
            {
                canDamage = false;
            }
#endif
            if (canDamage)
            {
                BattleStatus.TakeDamage(damage);
            }
            Events.OnTakeDamage.OnNext(damage);
            await sequences.PlayOnTakeDamageAsync(scope.Token);
        }

        public UniTask OnDeadMessageAsync(BattleCharacter target)
        {
            return sequences.PlayOnDeadMessageAsync(this, target, scope.Token);
        }

        public UniTask RecoveryHitPointAsync(int recovery)
        {
            BattleStatus.RecoveryHitPoint(recovery);
            return UniTask.CompletedTask;
        }

        public UniTask BeginBattleAsync(BattleCharacter target)
        {
            if (Equipment == null)
            {
                return UniTask.CompletedTask;
            }
            return Equipment.BeginBattleAsync(this, target, scope.Token);
        }
    }
}
