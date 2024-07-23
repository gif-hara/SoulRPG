namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class InputController
    {
        private readonly InputActions inputActions = new();

        public InputController()
        {
            inputActions.Enable();
        }
    }
}
