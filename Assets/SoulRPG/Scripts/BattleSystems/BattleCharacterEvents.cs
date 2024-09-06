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

        public readonly Subject<MasterData.Ailment> OnAddAilment = new();

        public readonly Subject<MasterData.Ailment> OnRemoveAilment = new();

        public readonly Subject<int> OnRecoveryHitPoint = new();
    }
}
