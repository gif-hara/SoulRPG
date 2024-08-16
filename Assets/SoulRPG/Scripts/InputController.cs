using System.Collections.Generic;
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
            switch (inputType)
            {
                case InputType.InGame:
                    inputActions.InGame.Enable();
                    inputActions.UI.Disable();
                    break;
                case InputType.UI:
                    inputActions.InGame.Disable();
                    inputActions.UI.Enable();
                    break;
            }
        }
    }
}
