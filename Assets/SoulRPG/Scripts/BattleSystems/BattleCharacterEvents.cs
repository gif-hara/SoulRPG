using R3;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class BattleCharacterEvents
    {
        public readonly Subject<int> OnTakeDamage = new();

        public readonly Subject<Unit> OnDeadMessage = new();
    }
}
