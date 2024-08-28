using HK;
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

        public bool IsPinch => ((float)HitPoint / HitPointMax) < TinyServiceLocator.Resolve<GameRule>().PinchThreshold;

        public CharacterInstanceStatus(Character character, CharacterGrowthParameter growthParameter)
        {
            hitPointMaxReactiveProperty = new ReactiveProperty<int>(GetHitPointMax());
            hitPointReactiveProperty = new ReactiveProperty<int>(GetHitPointMax());
            guardPointReactiveProperty = new ReactiveProperty<int>(0);
            staminaMaxReactiveProperty = new ReactiveProperty<int>(GetStaminaMax());
            staminaReactiveProperty = new ReactiveProperty<int>(GetStaminaMax());
            experienceReactiveProperty = new ReactiveProperty<int>(0);

            Observable.Merge(
                character.Events.OnLevelUp.AsUnitObservable(),
                character.Events.OnChangedEquipment.AsUnitObservable(),
                character.Events.OnSyncGrowthParameter.AsUnitObservable()
            )
                .Subscribe(_ =>
                {
                    SetHitPointMax(GetHitPointMax());
                    SetStaminaMax(GetStaminaMax());
                })
                .RegisterTo(character.LifeScope);

            int GetHitPointMax() =>
                (growthParameter.Vitality + growthParameter.Level + character.Equipment.TotalVitality)
                * TinyServiceLocator.Resolve<GameRule>().HitPointAmountRate;
            int GetStaminaMax() =>
                (growthParameter.Stamina + growthParameter.Level + character.Equipment.TotalStamina)
                * TinyServiceLocator.Resolve<GameRule>().StaminaAmountRate;
        }

        public void SetHitPointMax(int value)
        {
            hitPointMaxReactiveProperty.Value = value;
            if (hitPointReactiveProperty.Value > value)
            {
                hitPointReactiveProperty.Value = value;
            }
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
            if (staminaReactiveProperty.Value > value)
            {
                staminaReactiveProperty.Value = value;
            }
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
