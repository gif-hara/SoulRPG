using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.BattleSystems;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameEvents
    {
        public readonly Subject<ShowMessageData> OnRequestShowMessage = new();

        public readonly Subject<Unit> OnSubmitInput = new();

        public readonly Subject<bool> OnRequestSetActiveMessageArrow = new();

        public readonly Subject<DungeonInstanceFloorData> OnAcquiredFloorData = new();

        public readonly Subject<HashSet<string>> OnClearTemporaryCompletedEventIds = new();

        public readonly Subject<Vector2Int> OnAddReachedPoint = new();

        public readonly Subject<string> OnRequestPlaySfx = new();

        public readonly Subject<string> OnRequestPlayBgm = new();

        public readonly Subject<BattleSystem> OnBeginBattle = new();

#if DEBUG
        public readonly Subject<(string key, string message)> OnRequestAddDebugPanelInformation = new();

        public readonly Subject<string> OnRequestRemoveDebugPanelInformation = new();
#endif

        public UniTask ShowMessageAndWaitForSubmitInputAsync(ShowMessageData data)
        {
            OnRequestShowMessage.OnNext(data);
            return WaitForSubmitInputAsync();
        }

        public async UniTask WaitForSubmitInputAsync()
        {
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
            OnRequestSetActiveMessageArrow.OnNext(true);
            await OnSubmitInput.FirstAsync();
            OnRequestSetActiveMessageArrow.OnNext(false);
            inputController.ChangeInputType(tempInputType);
        }
    }
}
