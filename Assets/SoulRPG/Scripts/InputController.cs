using System.Collections.Generic;
using System.Text;
using System.Threading;
using HK;
using R3;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class InputController
    {
        private readonly InputActions inputActions = new();

        public InputActions InputActions => inputActions;

        private readonly Stack<InputType> inputTypeStack = new();

        public Subject<InputType> OnChangeInputType = new();

        private CancellationTokenSource inputGuideScope;

        public enum InputType
        {
            InGame,
            UI,
        }

        public InputController()
        {
            inputActions.Enable();
        }

        public void PushInputType(InputType inputType)
        {
            inputTypeStack.Push(inputType);
            ChangeInputType(inputType);
        }

        public void PushInputType(InputType inputType, CancellationToken scope)
        {
            PushInputType(inputType);
            scope.Register(() => PopInputType());
        }

        public void PopInputType()
        {
            if (inputTypeStack.Count <= 0)
            {
                return;
            }
            inputTypeStack.Pop();
            Assert.IsTrue(inputTypeStack.Count > 0);
            ChangeInputType(inputTypeStack.Peek());
        }

        private void ChangeInputType(InputType inputType)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            inputGuideScope?.Cancel();
            inputGuideScope?.Dispose();
            inputGuideScope = new CancellationTokenSource();
            var sb = new StringBuilder();
            switch (inputType)
            {
                case InputType.InGame:
                    inputActions.InGame.Enable();
                    inputActions.UI.Disable();
                    // gameEvents.OnRequestShowInputGuideBottom.OnNext((() =>
                    // {
                    //     return
                    //         inputActions.InGame.Move.GetTag() + ":移動" +
                    //         inputActions.InGame.Shift.GetTag() + ":水平移動" +
                    //         inputActions.InGame.ToMenu.GetTag() + ":メニュー" +
                    //         inputActions.InGame.ToggleMiniMapView.GetTag() + ":マップ切り替え";
                    // }, inputGuideScope.Token));
                    break;
                case InputType.UI:
                    inputActions.InGame.Disable();
                    inputActions.UI.Enable();
                    // gameEvents.OnRequestShowInputGuideBottom.OnNext((() => "UI", inputGuideScope.Token));
                    break;
            }
            OnChangeInputType.OnNext(inputType);
        }
    }
}
