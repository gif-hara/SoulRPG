using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

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
        }

        public async UniTask AddAsync(int masterDataAilmentId, int turnCount)
        {
            var element = elements.Find(x => x.GetMasterDataId() == masterDataAilmentId);
            if (element != null)
            {
                element.Reset(turnCount);
            }
            else
            {
                element = new AilmentElement(masterDataAilmentId, turnCount);
                elements.Add(element);
                await element.OnAddedAsync(battleCharacter, cancellationTokenSource.Token);
            }
        }

        public async UniTask OnTurnEndAsync()
        {
            foreach (var element in elements)
            {
                await element.OnTurnEndAsync(battleCharacter, cancellationTokenSource.Token);
                if (element.IsEnd())
                {
                    await element.OnRemovedAsync(battleCharacter, cancellationTokenSource.Token);
                }
            }
            elements.RemoveAll(x => x.IsEnd());
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

        public void Dispose()
        {
            cancellationTokenSource?.Dispose();
        }
    }
}
