using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class AilmentController : IDisposable
    {
        private readonly CancellationTokenSource cancellationTokenSource = new();

        private readonly List<IAilmentElement> elements = new();

        private readonly BattleCharacter battleCharacter;

        public IReadOnlyList<IAilmentElement> Elements => elements;

        public AilmentController(BattleCharacter battleCharacter)
        {
            this.battleCharacter = battleCharacter;
#if DEBUG
            AddDebugPanel();
#endif
        }

        public void Dispose()
        {
            cancellationTokenSource?.Dispose();
#if DEBUG
            RemoveDebugPanel();
#endif
        }

        public bool Contains(int masterDataAilmentId)
        {
            return elements.Exists(x => x.GetMasterDataId() == masterDataAilmentId);
        }

        public IAilmentElement Find(int masterDataAilmentId)
        {
            return elements.Find(x => x.GetMasterDataId() == masterDataAilmentId);
        }

        public bool ContainsDebuff()
        {
            return elements.Exists(x => x.GetMasterDataAilment().IsDebuff);
        }

        public int GetInvokeCountThisTurn(int masterDataAilmentId)
        {
            var element = elements.Find(x => x.GetMasterDataId() == masterDataAilmentId);
            return element?.GetInvokeCountThisTurn() ?? 0;
        }

        public void AddAilmentInvokeCountThisTurn(int masterDataAilmentId, int value)
        {
            var element = elements.Find(x => x.GetMasterDataId() == masterDataAilmentId);
            if (element == null)
            {
                return;
            }
            element.AddInvokeCountThisTurn(value);
        }

        public int GetInvokeCount(int masterDataAilmentId)
        {
            var element = elements.Find(x => x.GetMasterDataId() == masterDataAilmentId);
            return element?.GetInvokeCount() ?? 0;
        }

        public void AddAilmentInvokeCount(int masterDataAilmentId, int value)
        {
            var element = elements.Find(x => x.GetMasterDataId() == masterDataAilmentId);
            if (element == null)
            {
                return;
            }
            element.AddInvokeCount(value);
        }

        public async UniTask AddAsync(int masterDataAilmentId, int turnCount)
        {
            turnCount = await OnCalculateAilmentTurnCountAsync(turnCount);
            foreach (var i in elements)
            {
                var canAdd = await i.CanAddAilmentAsync(battleCharacter, masterDataAilmentId, cancellationTokenSource.Token);
                if (!canAdd)
                {
                    return;
                }
            }
            var element = elements.Find(x => x.GetMasterDataId() == masterDataAilmentId);
            if (element == null)
            {
                element = new AilmentElement(masterDataAilmentId, turnCount);
                elements.Add(element);
                battleCharacter.Events.OnAddAilment.OnNext(masterDataAilmentId.GetMasterDataAilment());
                await element.OnAddedAsync(battleCharacter, cancellationTokenSource.Token);
            }
            else if (masterDataAilmentId.GetMasterDataAilment().CanReset)
            {
                battleCharacter.Events.OnAddAilment.OnNext(masterDataAilmentId.GetMasterDataAilment());
                await element.OnResetAsync(battleCharacter, turnCount, cancellationTokenSource.Token);
            }
#if DEBUG
            AddDebugPanel();
#endif
        }

        public async UniTask RemoveAsync(int masterDataAilmentId)
        {
            var element = elements.Find(x => x.GetMasterDataId() == masterDataAilmentId);
            if (element == null)
            {
                return;
            }

            battleCharacter.Events.OnRemoveAilment.OnNext(masterDataAilmentId.GetMasterDataAilment());
            await element.OnRemovedAsync(battleCharacter, cancellationTokenSource.Token);
            elements.Remove(element);
        }

        public async UniTask OnTurnEndAsync(BattleCharacter target)
        {
            var tempElement = new List<IAilmentElement>(elements);
            foreach (var element in tempElement)
            {
                await element.OnTurnEndAsync(battleCharacter, target, cancellationTokenSource.Token);
                if (element.IsEnd())
                {
                    await element.OnRemovedAsync(battleCharacter, cancellationTokenSource.Token);
                }
            }
            elements.RemoveAll(x => x.IsEnd());
#if DEBUG
            AddDebugPanel();
#endif
        }

        public async UniTask<bool> CanExecutableTurnAsync()
        {
            foreach (var element in elements)
            {
                if (!await element.CanExecutableTurnAsync(battleCharacter, cancellationTokenSource.Token))
                {
                    return false;
                }
            }
            return true;
        }

        public async UniTask OnComboFromGivedDamageAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope)
        {
            scope = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, scope).Token;
            foreach (var element in elements)
            {
                await element.OnComboFromGivedDamageAsync(actor, target, scope);
            }
        }

        public async UniTask OnComboFromTakedDamageAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope)
        {
            scope = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, scope).Token;
            foreach (var element in elements)
            {
                await element.OnComboFromTakedDamageAsync(actor, target, scope);
            }
        }

        public async UniTask<bool> EvaluateEvadeAsync(BattleCharacter actor)
        {
            foreach (var element in elements)
            {
                if (await element.EvaluateEvadeAsync(actor, cancellationTokenSource.Token))
                {
                    return true;
                }
            }
            return false;
        }

        public async UniTask OnTakeDamageAsync(BattleCharacter actor, BattleCharacter target, DamageData damageData, CancellationToken scope)
        {
            scope = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, scope).Token;
            foreach (var element in elements)
            {
                await element.OnTakeDamageAsync(actor, target, damageData, scope);
            }
        }

        public async UniTask<int> OnCalculateNeedBehaviourPointAsync(int cost)
        {
            foreach (var element in elements)
            {
                cost = await element.OnCalculateNeedBehaviourPointAsync(battleCharacter, cost, cancellationTokenSource.Token);
            }
            return cost;
        }

        public async UniTask<int> OnCalculateNeedStaminaAsync(int cost)
        {
            foreach (var element in elements)
            {
                cost = await element.OnCalculateNeedStaminaAsync(battleCharacter, cost, cancellationTokenSource.Token);
            }
            return cost;
        }

        public async UniTask OnBehaviourEndAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope)
        {
            scope = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, scope).Token;
            var tempElements = new List<IAilmentElement>(elements);
            foreach (var element in tempElements)
            {
                await element.OnBehaviourEndAsync(actor, target, scope);
            }
        }

        public async UniTask OnTurnStartAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope)
        {
            scope = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, scope).Token;
            foreach (var element in elements)
            {
                await element.OnTurnStartAsync(actor, target, scope);
            }
        }

        public async UniTask<bool> CanExecutableTurnOpponentAsync(BattleCharacter target)
        {
            foreach (var element in elements)
            {
                if (!await element.CanExecutableTurnOpponentAsync(battleCharacter, target, cancellationTokenSource.Token))
                {
                    return false;
                }
            }
            return true;
        }

        public async UniTask<int> OnCalculateAilmentTurnCountAsync(int turnCount)
        {
            foreach (var element in elements)
            {
                turnCount = await element.OnCalculateAilmentTurnCountAsync(battleCharacter, turnCount, cancellationTokenSource.Token);
            }
            return turnCount;
        }

        public async UniTask<int> OnCalculateAddExperienceAsync(int experience)
        {
            foreach (var element in elements)
            {
                experience = await element.OnCalculateAddExperienceAsync(battleCharacter, experience, cancellationTokenSource.Token);
            }
            return experience;
        }

#if DEBUG
        private void AddDebugPanel()
        {
            var sb = new StringBuilder();
            sb.AppendLine("  - Ailment");
            foreach (var ailment in elements)
            {
                var masterDataAilment = ailment.GetMasterDataAilment();
                sb.AppendLine($"    - {masterDataAilment.Name.Localized()} [{ailment.GetRemainingTurnCount()}]");
            }
            TinyServiceLocator.Resolve<GameEvents>()
                .OnRequestAddDebugPanelInformation.OnNext(($"{battleCharacter.BattleStatus.Name}.AilmendController", sb.ToString()));
        }

        private void RemoveDebugPanel()
        {
            TinyServiceLocator.Resolve<GameEvents>()
                .OnRequestRemoveDebugPanelInformation.OnNext($"{battleCharacter.BattleStatus.Name}.AilmendController");
        }
#endif
    }
}
