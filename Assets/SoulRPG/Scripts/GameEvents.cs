using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameEvents
    {
        public readonly Subject<string> OnRequestShowMessage = new();

        public readonly Subject<Unit> OnSubmitInput = new();

        public readonly Subject<bool> OnRequestSetActiveMessageArrow = new();

        public readonly Subject<(string dungeonName, int x, int y)> OnAcquiredDungeonEvent = new();

        public readonly Subject<HashSet<string>> OnClearTemporaryCompletedEventIds = new();

        public readonly Subject<(string dungeonName, Vector2Int reachedPosition)> OnAddReachedPoint = new();

        public async UniTask WaitForSubmitInputAsync()
        {
            OnRequestSetActiveMessageArrow.OnNext(true);
            await OnSubmitInput.FirstAsync();
            OnRequestSetActiveMessageArrow.OnNext(false);
        }
    }
}
