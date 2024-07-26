namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class InputController
    {
        private readonly InputActions inputActions = new();

        public InputActions InputActions => inputActions;

        public enum InputType
        {
            InGame,
            UI,
        }

        public InputController()
        {
            inputActions.Enable();
        }

        public void ChangeInputType(InputType inputType)
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