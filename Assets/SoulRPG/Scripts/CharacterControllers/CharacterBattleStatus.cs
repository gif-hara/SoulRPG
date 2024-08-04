using System;
using System.Text;
using System.Threading;
using HK;
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

        private readonly ReactiveProperty<int> experienceReactiveProperty;
        public ReadOnlyReactiveProperty<int> ExperienceReactiveProperty => experienceReactiveProperty;
        public int Experience => experienceReactiveProperty.Value;

        private readonly ReactiveProperty<int> behaviourPointMaxReactiveProperty = new(0);
        public ReadOnlyReactiveProperty<int> BehaviorPointMaxReactiveProperty => behaviourPointMaxReactiveProperty;

        private readonly ReactiveProperty<int> behaviourPointReactiveProperty = new(0);
        public ReadOnlyReactiveProperty<int> BehaviourPointReactiveProperty => behaviourPointReactiveProperty;

        private readonly Define.CharacterAttribute attribute;
        public Define.CharacterAttribute Attribute => attribute;

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
            experienceReactiveProperty = new ReactiveProperty<int>(0);
            attribute = character.Attribute;

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
            behaviourPointMaxReactiveProperty.Value = 3;
            behaviourPointReactiveProperty.Value = behaviourPointMaxReactiveProperty.Value;
#if DEBUG
            AddDebugPanel();
#endif
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
            experienceReactiveProperty = new ReactiveProperty<int>(blueprint.Experience);
            behaviourPointMaxReactiveProperty = new ReactiveProperty<int>(blueprint.BehaviourPoint);
            behaviourPointReactiveProperty = new ReactiveProperty<int>(blueprint.BehaviourPoint);
            attribute = blueprint.Attribute;
#if DEBUG
            AddDebugPanel();
#endif
        }

        public CharacterBattleStatus(MasterData.Enemy enemy)
        {
            Name = enemy.Name;
            hitPointMaxReactiveProperty = new ReactiveProperty<int>(enemy.HitPoint);
            hitPointReactiveProperty = new ReactiveProperty<int>(enemy.HitPoint);
            staminaMaxReactiveProperty = new ReactiveProperty<int>(enemy.Stamina);
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
            experienceReactiveProperty = new ReactiveProperty<int>(enemy.Experience);
            behaviourPointMaxReactiveProperty = new ReactiveProperty<int>(enemy.BehaviourPoint);
            behaviourPointReactiveProperty = new ReactiveProperty<int>(enemy.BehaviourPoint);
            attribute = enemy.Attribute;
#if DEBUG
            AddDebugPanel();
#endif
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

        public void AddCutRate(Define.AttackAttribute attackAttribute, float value)
        {
            switch (attackAttribute)
            {
                case Define.AttackAttribute.Slash:
                    slashCutRateReactiveProperty.Value += value;
                    break;
                case Define.AttackAttribute.Blow:
                    blowCutRateReactiveProperty.Value += value;
                    break;
                case Define.AttackAttribute.Thrust:
                    thrustCutRateReactiveProperty.Value += value;
                    break;
                case Define.AttackAttribute.Magic:
                    magicCutRateReactiveProperty.Value += value;
                    break;
                case Define.AttackAttribute.Fire:
                    fireCutRateReactiveProperty.Value += value;
                    break;
                case Define.AttackAttribute.Thunder:
                    thunderCutRateReactiveProperty.Value += value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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

        public void AddBehaviourPoint(int value)
        {
            behaviourPointReactiveProperty.Value =
                Mathf.Clamp(behaviourPointReactiveProperty.Value + value, 0, behaviourPointMaxReactiveProperty.Value);
        }

        public void RecoveryBehaviourPoint()
        {
            behaviourPointReactiveProperty.Value = behaviourPointMaxReactiveProperty.Value;
        }

        public bool CanBehaviour()
        {
            return behaviourPointReactiveProperty.Value > 0;
        }

        public void Dispose()
        {
            scope.Cancel();
            scope.Dispose();
#if DEBUG
            RemoveDebugPanel();
#endif
        }

#if DEBUG
        private void AddDebugPanel()
        {
            Observable.Merge(
                hitPointReactiveProperty.AsUnitObservable(),
                staminaReactiveProperty.AsUnitObservable(),
                physicalAttackReactiveProperty.AsUnitObservable(),
                magicalAttackReactiveProperty.AsUnitObservable(),
                slashCutRateReactiveProperty.AsUnitObservable(),
                blowCutRateReactiveProperty.AsUnitObservable(),
                thrustCutRateReactiveProperty.AsUnitObservable(),
                magicCutRateReactiveProperty.AsUnitObservable(),
                fireCutRateReactiveProperty.AsUnitObservable(),
                thunderCutRateReactiveProperty.AsUnitObservable(),
                speedReactiveProperty.AsUnitObservable(),
                experienceReactiveProperty.AsUnitObservable(),
                behaviourPointMaxReactiveProperty.AsUnitObservable(),
                behaviourPointReactiveProperty.AsUnitObservable()
                )
                .Subscribe(_ =>
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("  - CharacterBattleStatus");
                    sb.AppendLine($"    - Name: {Name}");
                    sb.AppendLine($"    - HitPoint: {hitPointReactiveProperty.Value}/{hitPointMaxReactiveProperty.Value}");
                    sb.AppendLine($"    - Stamina: {staminaReactiveProperty.Value}/{staminaMaxReactiveProperty.Value}");
                    sb.AppendLine($"    - PhysicalAttack: {physicalAttackReactiveProperty.Value}");
                    sb.AppendLine($"    - MagicalAttack: {magicalAttackReactiveProperty.Value}");
                    sb.AppendLine($"    - SlashCutRate: {slashCutRateReactiveProperty.Value}");
                    sb.AppendLine($"    - BlowCutRate: {blowCutRateReactiveProperty.Value}");
                    sb.AppendLine($"    - ThrustCutRate: {thrustCutRateReactiveProperty.Value}");
                    sb.AppendLine($"    - MagicCutRate: {magicCutRateReactiveProperty.Value}");
                    sb.AppendLine($"    - FireCutRate: {fireCutRateReactiveProperty.Value}");
                    sb.AppendLine($"    - ThunderCutRate: {thunderCutRateReactiveProperty.Value}");
                    sb.AppendLine($"    - Speed: {speedReactiveProperty.Value}");
                    sb.AppendLine($"    - Experience: {experienceReactiveProperty.Value}");
                    sb.AppendLine($"    - BehaviourPoint: {behaviourPointReactiveProperty.Value}/{behaviourPointMaxReactiveProperty.Value}");
                    TinyServiceLocator.Resolve<GameEvents>().OnRequestAddDebugPanelInformation
                        .OnNext(($"{Name}.CharacterBattleStatus", sb.ToString()));
                })
                .RegisterTo(scope.Token);
        }

        private void RemoveDebugPanel()
        {
            TinyServiceLocator.Resolve<GameEvents>().OnRequestRemoveDebugPanelInformation
                .OnNext($"{Name}.CharacterBattleStatus");
        }
#endif
    }
}
