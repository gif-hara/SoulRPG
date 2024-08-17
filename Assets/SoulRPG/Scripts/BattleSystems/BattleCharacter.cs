using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using SoulRPG.BattleSystems.CommandInvokers;
using SoulRPG.CharacterControllers;
using UnityEngine;
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

        private IBattleAI battleAI;

        public Queue<(string, ICommandInvoker)> AfterCommandInvokers { get; private set; } = new();

        public AilmentController AilmentController { get; }

        public StatusBuffController StatusBuffController { get; } = new();

        public HashSet<string> UsedSkills { get; } = new();

        public BattleCharacterEvents Events { get; } = new();

        private readonly BattleCharacterSequences sequences;

        public BattleCharacter(Character character, Define.AllyType allyType, IBattleAI battleAI, BattleCharacterSequences sequences)
        {
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

        public async UniTask TurnStartAsync()
        {
            UsedSkills.Clear();
            var recoveryBehaviourPoint = BattleStatus.BehaviorPointMaxReactiveProperty.CurrentValue;
            recoveryBehaviourPoint = await AilmentController.OnCalculateBehaviourPointAsync(recoveryBehaviourPoint);
            BattleStatus.AddBehaviourPoint(recoveryBehaviourPoint);
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
            battleAI.Dispose();
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

        public UniTask<bool> EvaluateEvaded()
        {
            return AilmentController.EvaluateEvadeAsync(this);
        }

        public async UniTask<int> GetFixedNeedBehaviourPointAsync(int cost)
        {
            var result = await AilmentController.OnCalculateNeedBehaviourPointAsync(cost);
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

        public void TakeDamage(int damage)
        {
#if DEBUG
            var battleDebugData = TinyServiceLocator.Resolve<BattleDebugData>();
            if (BattleStatus.AllyType == Define.AllyType.Player && battleDebugData.IsInvinciblePlayer)
            {
                return;
            }
            if (BattleStatus.AllyType == Define.AllyType.Enemy && battleDebugData.IsInvincibleEnemy)
            {
                return;
            }
#endif
            BattleStatus.TakeDamage(damage);
            Events.OnTakeDamage.OnNext(damage);
        }
    }
}
