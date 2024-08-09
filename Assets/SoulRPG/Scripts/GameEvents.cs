using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HK;
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

        public readonly Subject<Vector2Int> OnAddReachedPoint = new();

#if DEBUG
        public readonly Subject<(string key, string message)> OnRequestAddDebugPanelInformation = new();

        public readonly Subject<string> OnRequestRemoveDebugPanelInformation = new();
#endif

        public async UniTask ShowMessageAndWaitForSubmitInputAsync(string message)
        {
            OnRequestShowMessage.OnNext(message);
            var inputController = TinyServiceLocator.Resolve<InputController>();
            var tempInputType = inputController.CurrentInputType;
            await UniTask.NextFrame();
            inputController.ChangeInputType(InputController.InputType.UI);
            inputController.InputActions.UI.Submit.OnPerformedAsObservable()
                .Take(1)
                .Subscribe(_ =>
                {
                    OnSubmitInput.OnNext(Unit.Default);
                });
            await WaitForSubmitInputAsync();
            inputController.ChangeInputType(tempInputType);
        }

        public async UniTask WaitForSubmitInputAsync()
        {
            OnRequestSetActiveMessageArrow.OnNext(true);
            await OnSubmitInput.FirstAsync();
            OnRequestSetActiveMessageArrow.OnNext(false);
        }
    }
}
