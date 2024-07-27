using System;
using R3;
using SoulRPG.CharacterControllers;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CharacterBattleStatus
    {
        public string Name { get; }

        private readonly ReactiveProperty<int> hitPointReactiveProperty;
        public ReadOnlyReactiveProperty<int> HitPoint => hitPointReactiveProperty;

        private readonly ReactiveProperty<int> staminaReactiveProperty;
        public ReadOnlyReactiveProperty<int> Stamina => staminaReactiveProperty;

        private readonly ReactiveProperty<int> physicalAttackReactiveProperty;
        public ReadOnlyReactiveProperty<int> PhysicalAttack => physicalAttackReactiveProperty;

        private readonly ReactiveProperty<int> magicalAttackReactiveProperty;
        public ReadOnlyReactiveProperty<int> MagicalAttack => magicalAttackReactiveProperty;

        private readonly ReactiveProperty<float> slashCutRateReactiveProperty;
        public ReadOnlyReactiveProperty<float> SlashCutRate => slashCutRateReactiveProperty;

        private readonly ReactiveProperty<float> blowCutRateReactiveProperty;
        public ReadOnlyReactiveProperty<float> BlowCutRate => blowCutRateReactiveProperty;

        private readonly ReactiveProperty<float> thrustCutRateReactiveProperty;
        public ReadOnlyReactiveProperty<float> ThrustCutRate => thrustCutRateReactiveProperty;

        private readonly ReactiveProperty<float> magicCutRateReactiveProperty;
        public ReadOnlyReactiveProperty<float> MagicCutRate => magicCutRateReactiveProperty;

        private readonly ReactiveProperty<float> fireCutRateReactiveProperty;
        public ReadOnlyReactiveProperty<float> FireCutRate => fireCutRateReactiveProperty;

        private readonly ReactiveProperty<float> thunderCutRateReactiveProperty;
        public ReadOnlyReactiveProperty<float> ThunderCutRate => thunderCutRateReactiveProperty;

        private readonly ReactiveProperty<int> speedReactiveProperty;
        public ReadOnlyReactiveProperty<int> Speed => speedReactiveProperty;

        public bool IsDead => hitPointReactiveProperty.Value <= 0;

        public CharacterBattleStatus(Character character)
        {
            var growthParameter = character.GrowthParameter;
            var equipment = character.Equipment;
            Name = character.Name;
            hitPointReactiveProperty = new ReactiveProperty<int>(growthParameter.Vitality * 8);
            staminaReactiveProperty = new ReactiveProperty<int>(growthParameter.Stamina);
            physicalAttackReactiveProperty = new ReactiveProperty<int>(growthParameter.PhysicalStrength);
            magicalAttackReactiveProperty = new ReactiveProperty<int>(growthParameter.MagicalStrength);
            slashCutRateReactiveProperty = new ReactiveProperty<float>(equipment.TotalSlashCutRate);
            blowCutRateReactiveProperty = new ReactiveProperty<float>(equipment.TotalBlowCutRate);
            thrustCutRateReactiveProperty = new ReactiveProperty<float>(equipment.TotalThrustCutRate);
            magicCutRateReactiveProperty = new ReactiveProperty<float>(equipment.TotalMagicCutRate);
            fireCutRateReactiveProperty = new ReactiveProperty<float>(equipment.TotalFireCutRate);
            thunderCutRateReactiveProperty = new ReactiveProperty<float>(equipment.TotalThunderCutRate);
            speedReactiveProperty = new ReactiveProperty<int>(growthParameter.Speed);
        }

        public CharacterBattleStatus(CharacterBattleStatusBlueprint blueprint)
        {
            Name = blueprint.Name;
            hitPointReactiveProperty = new ReactiveProperty<int>(blueprint.HitPoint);
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
    }
}
