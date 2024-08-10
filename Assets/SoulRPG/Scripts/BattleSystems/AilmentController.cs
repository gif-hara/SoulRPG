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

        public async UniTask AddAsync(int masterDataAilmentId, int turnCount)
        {
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
                await element.OnAddedAsync(battleCharacter, cancellationTokenSource.Token);
            }
            else if (masterDataAilmentId.GetMasterDataAilment().CanReset)
            {
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

            await element.OnRemovedAsync(battleCharacter, cancellationTokenSource.Token);
            elements.Remove(element);
        }

        public async UniTask OnTurnEndAsync()
        {
            var tempElement = new List<IAilmentElement>(elements);
            foreach (var element in tempElement)
            {
                await element.OnTurnEndAsync(battleCharacter, cancellationTokenSource.Token);
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

        public async UniTask OnTakeDamageAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope)
        {
            scope = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, scope).Token;
            foreach (var element in elements)
            {
                await element.OnTakeDamageAsync(actor, target, scope);
            }
        }

        public async UniTask<int> OnCalculateBehaviourPointAsync(int behaviourPoint)
        {
            foreach (var element in elements)
            {
                behaviourPoint = await element.OnCalculateBehaviourPointAsync(battleCharacter, behaviourPoint, cancellationTokenSource.Token);
            }
            return behaviourPoint;
        }

        public async UniTask<int> OnCalculateNeedBehaviourPointAsync(int cost)
        {
            foreach (var element in elements)
            {
                cost = await element.OnCalculateNeedBehaviourPointAsync(battleCharacter, cost, cancellationTokenSource.Token);
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

#if DEBUG
        private void AddDebugPanel()
        {
            var sb = new StringBuilder();
            sb.AppendLine("  - Ailment");
            foreach (var ailment in elements)
            {
                var masterDataAilment = ailment.GetMasterDataAilment();
                sb.AppendLine($"    - {masterDataAilment.Name} [{ailment.GetRemainingTurnCount()}]");
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
