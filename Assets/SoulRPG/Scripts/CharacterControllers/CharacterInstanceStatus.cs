using R3;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CharacterInstanceStatus
    {
        private readonly ReactiveProperty<int> hitPointMaxReactiveProperty;
        public ReadOnlyReactiveProperty<int> HitPointMaxAsObservable() => hitPointMaxReactiveProperty;
        public int HitPointMax => hitPointMaxReactiveProperty.Value;

        private readonly ReactiveProperty<int> hitPointReactiveProperty;
        public ReadOnlyReactiveProperty<int> HitPointAsObservable() => hitPointReactiveProperty;
        public int HitPoint => hitPointReactiveProperty.Value;
        
        private readonly ReactiveProperty<int> guardPointReactiveProperty;
        public ReadOnlyReactiveProperty<int> GuardPointAsObservable() => guardPointReactiveProperty;
        public int GuardPoint => guardPointReactiveProperty.Value;

        private readonly ReactiveProperty<int> staminaMaxReactiveProperty;
        public ReadOnlyReactiveProperty<int> StaminaMaxAsObservable() => staminaMaxReactiveProperty;
        public int StaminaMax => staminaMaxReactiveProperty.Value;

        private readonly ReactiveProperty<int> staminaReactiveProperty;
        public ReadOnlyReactiveProperty<int> StaminaAsObservable() => staminaReactiveProperty;
        public int Stamina => staminaReactiveProperty.Value;

        public CharacterInstanceStatus(CharacterGrowthParameter growthParameter)
        {
            hitPointMaxReactiveProperty = new ReactiveProperty<int>(growthParameter.HitPoint);
            hitPointReactiveProperty = new ReactiveProperty<int>(growthParameter.HitPoint);
            guardPointReactiveProperty = new ReactiveProperty<int>(0);
            staminaMaxReactiveProperty = new ReactiveProperty<int>(growthParameter.Stamina);
            staminaReactiveProperty = new ReactiveProperty<int>(growthParameter.Stamina);
        }

        public void SetHitPointMax(int value)
        {
            hitPointMaxReactiveProperty.Value = value;
        }

        public void SetHitPoint(int value)
        {
            hitPointReactiveProperty.Value = value;
        }

        public void SetStaminaMax(int value)
        {
            staminaMaxReactiveProperty.Value = value;
        }

        public void SetStamina(int value)
        {
            staminaReactiveProperty.Value = value;
        }

        public void FullRecovery()
        {
            hitPointReactiveProperty.Value = hitPointMaxReactiveProperty.Value;
            staminaReactiveProperty.Value = staminaMaxReactiveProperty.Value;
        }
    }
}
