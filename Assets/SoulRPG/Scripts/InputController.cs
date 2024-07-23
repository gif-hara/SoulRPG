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
    }
}
