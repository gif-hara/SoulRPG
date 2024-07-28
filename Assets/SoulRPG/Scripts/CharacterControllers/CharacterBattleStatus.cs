using System;
using System.Threading;
using R3;
using SoulRPG.CharacterControllers;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CharacterBattleStatus : IDisposable
    {
        public string Name { get; }

        private readonly ReactiveProperty<int> hitPointMaxReactiveProperty;
        public ReadOnlyReactiveProperty<int> HitPointMaxReactiveProperty => hitPointMaxReactiveProperty;
        public int HitPointMax => hitPointMaxReactiveProperty.Value;

        private readonly ReactiveProperty<int> hitPointReactiveProperty;
        public ReadOnlyReactiveProperty<int> HitPointReactiveProperty => hitPointReactiveProperty;
        public int HitPoint => hitPointReactiveProperty.Value;

        private readonly ReactiveProperty<int> staminaMaxReactiveProperty;
        public ReadOnlyReactiveProperty<int> StaminaMaxReactiveProperty => staminaMaxReactiveProperty;
        public int StaminaMax => staminaMaxReactiveProperty.Value;

        private readonly ReactiveProperty<int> staminaReactiveProperty;
        public ReadOnlyReactiveProperty<int> StaminaReactiveProperty => staminaReactiveProperty;
        public int Stamina => staminaReactiveProperty.Value;

        private readonly ReactiveProperty<int> physicalAttackReactiveProperty;
        public ReadOnlyReactiveProperty<int> PhysicalAttackReactiveProperty => physicalAttackReactiveProperty;
        public int PhysicalAttack => physicalAttackReactiveProperty.Value;

        private readonly ReactiveProperty<int> magicalAttackReactiveProperty;
        public ReadOnlyReactiveProperty<int> MagicalAttackReactiveProperty => magicalAttackReactiveProperty;
        public int MagicalAttack => magicalAttackReactiveProperty.Value;

        private readonly ReactiveProperty<float> slashCutRateReactiveProperty;
        public ReadOnlyReactiveProperty<float> SlashCutRateReactiveProperty => slashCutRateReactiveProperty;
        public float SlashCutRate => slashCutRateReactiveProperty.Value;

        private readonly ReactiveProperty<float> blowCutRateReactiveProperty;
        public ReadOnlyReactiveProperty<float> BlowCutRateReactiveProperty => blowCutRateReactiveProperty;
        public float BlowCutRate => blowCutRateReactiveProperty.Value;

        private readonly ReactiveProperty<float> thrustCutRateReactiveProperty;
        public ReadOnlyReactiveProperty<float> ThrustCutRateReactiveProperty => thrustCutRateReactiveProperty;
        public float ThrustCutRate => thrustCutRateReactiveProperty.Value;

        private readonly ReactiveProperty<float> magicCutRateReactiveProperty;
        public ReadOnlyReactiveProperty<float> MagicCutRateReactiveProperty => magicCutRateReactiveProperty;
        public float MagicCutRate => magicCutRateReactiveProperty.Value;

        private readonly ReactiveProperty<float> fireCutRateReactiveProperty;
        public ReadOnlyReactiveProperty<float> FireCutRateReactiveProperty => fireCutRateReactiveProperty;
        public float FireCutRate => fireCutRateReactiveProperty.Value;

        private readonly ReactiveProperty<float> thunderCutRateReactiveProperty;
        public ReadOnlyReactiveProperty<float> ThunderCutRateReactiveProperty => thunderCutRateReactiveProperty;
        public float ThunderCutRate => thunderCutRateReactiveProperty.Value;

        private readonly ReactiveProperty<int> speedReactiveProperty;
        public ReadOnlyReactiveProperty<int> SpeedReactiveProperty => speedReactiveProperty;
        public int Speed => speedReactiveProperty.Value;

        public bool IsDead => hitPointReactiveProperty.Value <= 0;

        private readonly CancellationTokenSource scope = new();

        public CharacterBattleStatus(Character character)
        {
            var growthParameter = character.GrowthParameter;
            var equipment = character.Equipment;
            var instanceStatus = character.InstanceStatus;
            Name = character.Name;
            hitPointMaxReactiveProperty = new ReactiveProperty<int>(instanceStatus.HitPointMax);
            hitPointReactiveProperty = new ReactiveProperty<int>(instanceStatus.HitPoint);
            staminaMaxReactiveProperty = new ReactiveProperty<int>(instanceStatus.StaminaMax);
            staminaReactiveProperty = new ReactiveProperty<int>(instanceStatus.Stamina);
            physicalAttackReactiveProperty = new ReactiveProperty<int>(growthParameter.PhysicalStrength);
            magicalAttackReactiveProperty = new ReactiveProperty<int>(growthParameter.MagicalStrength);
            slashCutRateReactiveProperty = new ReactiveProperty<float>(equipment.TotalSlashCutRate);
            blowCutRateReactiveProperty = new ReactiveProperty<float>(equipment.TotalBlowCutRate);
            thrustCutRateReactiveProperty = new ReactiveProperty<float>(equipment.TotalThrustCutRate);
            magicCutRateReactiveProperty = new ReactiveProperty<float>(equipment.TotalMagicCutRate);
            fireCutRateReactiveProperty = new ReactiveProperty<float>(equipment.TotalFireCutRate);
            thunderCutRateReactiveProperty = new ReactiveProperty<float>(equipment.TotalThunderCutRate);
            speedReactiveProperty = new ReactiveProperty<int>(growthParameter.Speed);

            hitPointReactiveProperty
                .Subscribe(x =>
                {
                    character.InstanceStatus.SetHitPoint(x);
                })
                .RegisterTo(scope.Token);
            staminaReactiveProperty
                .Subscribe(x =>
                {
                    character.InstanceStatus.SetStamina(x);
                })
                .RegisterTo(scope.Token);
        }

        public CharacterBattleStatus(CharacterBattleStatusBlueprint blueprint)
        {
            Name = blueprint.Name;
            hitPointMaxReactiveProperty = new ReactiveProperty<int>(blueprint.HitPoint);
            hitPointReactiveProperty = new ReactiveProperty<int>(blueprint.HitPoint);
            staminaMaxReactiveProperty = new ReactiveProperty<int>(blueprint.Stamina);
            staminaReactiveProperty = new ReactiveProperty<int>(blueprint.Stamina);
            physicalAttackReactiveProperty = new ReactiveProperty<int>(blueprint.PhysicalAttack);
            magicalAttackReactiveProperty = new ReactiveProperty<int>(blueprint.MagicalAttack);
            slashCutRateReactiveProperty = new ReactiveProperty<float>(blueprint.SlashCutRate);
            blowCutRateReactiveProperty = new ReactiveProperty<float>(blueprint.BlowCutRate);
            thrustCutRateReactiveProperty = new ReactiveProperty<float>(blueprint.ThrustCutRate);
            magicCutRateReactiveProperty = new ReactiveProperty<float>(blueprint.MagicCutRate);
            fireCutRateReactiveProperty = new ReactiveProperty<float>(blueprint.FireCutRate);
            thunderCutRateReactiveProperty = new ReactiveProperty<float>(blueprint.ThunderCutRate);
            speedReactiveProperty = new ReactiveProperty<int>(blueprint.Speed);
        }

        public CharacterBattleStatus(MasterData.Enemy enemy)
        {
            Name = enemy.Name;
            hitPointReactiveProperty = new ReactiveProperty<int>(enemy.HitPoint);
            staminaReactiveProperty = new ReactiveProperty<int>(enemy.Stamina);
            physicalAttackReactiveProperty = new ReactiveProperty<int>(enemy.PhysicalAttack);
            magicalAttackReactiveProperty = new ReactiveProperty<int>(enemy.MagicalAttack);
            slashCutRateReactiveProperty = new ReactiveProperty<float>(enemy.SlashCutRate);
            blowCutRateReactiveProperty = new ReactiveProperty<float>(enemy.BlowCutRate);
            thrustCutRateReactiveProperty = new ReactiveProperty<float>(enemy.ThrustCutRate);
            magicCutRateReactiveProperty = new ReactiveProperty<float>(enemy.MagicCutRate);
            fireCutRateReactiveProperty = new ReactiveProperty<float>(enemy.FireCutRate);
            thunderCutRateReactiveProperty = new ReactiveProperty<float>(enemy.ThunderCutRate);
            speedReactiveProperty = new ReactiveProperty<int>(enemy.Speed);
        }

        public void TakeDamage(int damage)
        {
            hitPointReactiveProperty.Value = Mathf.Max(0, hitPointReactiveProperty.Value - damage);
        }

        public int GetAttackPower(Define.AttackType attackType)
        {
            return attackType switch
            {
                Define.AttackType.Physical => physicalAttackReactiveProperty.Value,
                Define.AttackType.Magical => magicalAttackReactiveProperty.Value,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        public float GetCutRate(Define.AttackAttribute attackAttribute)
        {
            return attackAttribute switch
            {
                Define.AttackAttribute.Slash => slashCutRateReactiveProperty.Value,
                Define.AttackAttribute.Blow => blowCutRateReactiveProperty.Value,
                Define.AttackAttribute.Thrust => thrustCutRateReactiveProperty.Value,
                Define.AttackAttribute.Magic => magicCutRateReactiveProperty.Value,
                Define.AttackAttribute.Fire => fireCutRateReactiveProperty.Value,
                Define.AttackAttribute.Thunder => thunderCutRateReactiveProperty.Value,
                _ => throw new ArgumentOutOfRangeException(),
            };

        }

        public void Dispose()
        {
            scope.Cancel();
            scope.Dispose();
        }
    }
}
