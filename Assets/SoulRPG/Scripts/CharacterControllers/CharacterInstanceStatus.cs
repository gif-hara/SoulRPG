using R3;
using SoulRPG.CharacterControllers;
using UnityEngine;

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

        private readonly ReactiveProperty<int> experienceReactiveProperty;
        public ReadOnlyReactiveProperty<int> ExperienceAsObservable() => experienceReactiveProperty;
        public int Experience => experienceReactiveProperty.Value;

        public CharacterInstanceStatus(Character character, CharacterGrowthParameter growthParameter)
        {
            hitPointMaxReactiveProperty = new ReactiveProperty<int>(growthParameter.HitPointMax);
            hitPointReactiveProperty = new ReactiveProperty<int>(growthParameter.HitPointMax);
            guardPointReactiveProperty = new ReactiveProperty<int>(0);
            staminaMaxReactiveProperty = new ReactiveProperty<int>(growthParameter.StaminaMax);
            staminaReactiveProperty = new ReactiveProperty<int>(growthParameter.StaminaMax);
            experienceReactiveProperty = new ReactiveProperty<int>(0);

            character.Events.OnLevelUp
                .Subscribe(_ =>
                {
                    SetHitPointMax(growthParameter.HitPointMax);
                    SetStaminaMax(growthParameter.StaminaMax);
                })
                .RegisterTo(character.LifeScope);
        }

        public void SetHitPointMax(int value)
        {
            hitPointMaxReactiveProperty.Value = value;
        }

        public void SetHitPoint(int value)
        {
            hitPointReactiveProperty.Value = value;
        }

        public void SetGuardPoint(int value)
        {
            guardPointReactiveProperty.Value = value;
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

        public void ResetGuardPoint()
        {
            guardPointReactiveProperty.Value = 0;
        }

        public void AddExperience(int value)
        {
            experienceReactiveProperty.Value += value;
        }
    }
}
