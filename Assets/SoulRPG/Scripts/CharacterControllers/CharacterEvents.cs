using R3;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CharacterEvents
    {
        public readonly Subject<(int itemId, int count)> OnAcquiredItem = new();

        public readonly Subject<CharacterGrowthParameter> OnLevelUp = new();
    }
}
