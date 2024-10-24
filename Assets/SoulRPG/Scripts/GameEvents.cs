using System;
using System.Collections.Generic;
using System.Threading;
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

        public readonly Subject<Unit> OnEndBattle = new();

        public readonly Subject<Sprite> OnRequestChangeEnemySprite = new();

        public readonly Subject<(Func<string> messageSelector, CancellationToken scope)> OnRequestShowInputGuideCenter = new();

        public readonly Subject<(Func<string> messageSelector, CancellationToken scope)> OnRequestShowInputGuideBottom = new();

        public readonly Subject<Unit> OnOpenDoor = new();

        public readonly Subject<Define.MiniMapType> OnRequestChangeMiniMapType = new();

        public readonly Subject<Unit> OnRequestToggleMiniMapType = new();

        public readonly Subject<string> OnRequestChangeDungeon = new();

        public readonly Subject<int> OnRequestNextFloor = new();

        public readonly Subject<DungeonController> OnSetupDungeon = new();

        public readonly Subject<bool> OnChangeIsRotationMiniMap = new();

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
            await UniTask.NextFrame();
            inputController.PushInputType(InputController.InputType.UI);
            inputController.InputActions.UI.Submit.OnPerformedAsObservable()
                .Take(1)
                .Subscribe(_ =>
                {
                    OnSubmitInput.OnNext(Unit.Default);
                });
            OnRequestSetActiveMessageArrow.OnNext(true);
            await OnSubmitInput.FirstAsync();
            OnRequestSetActiveMessageArrow.OnNext(false);
            inputController.PopInputType();
        }
    }
}
