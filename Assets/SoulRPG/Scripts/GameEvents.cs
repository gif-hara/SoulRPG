using R3;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameEvents
    {
        public readonly Subject<string> OnRequestShowMessage = new();
    }
}
