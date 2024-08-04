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

        public CharacterBattleStatus
        (
            string name,
            int hitPointMax,
            int hitPoint,
            int staminaMax,
            int stamina,
            int physicalAttack,
            int magicalAttack,
            float slashCutRate,
            float blowCutRate,
            float thrustCutRate,
            float magicCutRate,
            float fireCutRate,
            float thunderCutRate,
            int speed,
            int experience,
            int behaviourPointMax,
            int behaviourPoint,
            Define.CharacterAttribute attribute
        )
        {
            Name = name;
            hitPointMaxReactiveProperty = new ReactiveProperty<int>(hitPointMax);
            hitPointReactiveProperty = new ReactiveProperty<int>(hitPoint);
            staminaMaxReactiveProperty = new ReactiveProperty<int>(staminaMax);
            staminaReactiveProperty = new ReactiveProperty<int>(stamina);
            physicalAttackReactiveProperty = new ReactiveProperty<int>(physicalAttack);
            magicalAttackReactiveProperty = new ReactiveProperty<int>(magicalAttack);
            slashCutRateReactiveProperty = new ReactiveProperty<float>(slashCutRate);
            blowCutRateReactiveProperty = new ReactiveProperty<float>(blowCutRate);
            thrustCutRateReactiveProperty = new ReactiveProperty<float>(thrustCutRate);
            magicCutRateReactiveProperty = new ReactiveProperty<float>(magicCutRate);
            fireCutRateReactiveProperty = new ReactiveProperty<float>(fireCutRate);
            thunderCutRateReactiveProperty = new ReactiveProperty<float>(thunderCutRate);
            speedReactiveProperty = new ReactiveProperty<int>(speed);
            experienceReactiveProperty = new ReactiveProperty<int>(experience);
            behaviourPointMaxReactiveProperty = new ReactiveProperty<int>(behaviourPointMax);
            behaviourPointReactiveProperty = new ReactiveProperty<int>(behaviourPoint);
            this.attribute = attribute;
#if DEBUG
            AddDebugPanel();
#endif
        }

        public CharacterBattleStatus(Character character)
            : this
            (
                character.Name,
                character.InstanceStatus.HitPointMax,
                character.InstanceStatus.HitPoint,
                character.InstanceStatus.StaminaMax,
                character.InstanceStatus.Stamina,
                character.GrowthParameter.PhysicalStrength,
                character.GrowthParameter.MagicalStrength,
                character.Equipment.TotalSlashCutRate,
                character.Equipment.TotalBlowCutRate,
                character.Equipment.TotalThrustCutRate,
                character.Equipment.TotalMagicCutRate,
                character.Equipment.TotalFireCutRate,
                character.Equipment.TotalThunderCutRate,
                character.GrowthParameter.Speed,
                0,
                3,
                3,
                character.Attribute
            )
        {
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
            : this
            (
                blueprint.Name,
                blueprint.HitPoint,
                blueprint.HitPoint,
                blueprint.Stamina,
                blueprint.Stamina,
                blueprint.PhysicalAttack,
                blueprint.MagicalAttack,
                blueprint.SlashCutRate,
                blueprint.BlowCutRate,
                blueprint.ThrustCutRate,
                blueprint.MagicCutRate,
                blueprint.FireCutRate,
                blueprint.ThunderCutRate,
                blueprint.Speed,
                blueprint.Experience,
                blueprint.BehaviourPoint,
                blueprint.BehaviourPoint,
                blueprint.Attribute
            )
        {
        }

        public CharacterBattleStatus(MasterData.Enemy enemy)
            : this
            (
                enemy.Name,
                enemy.HitPoint,
                enemy.HitPoint,
                enemy.Stamina,
                enemy.Stamina,
                enemy.PhysicalAttack,
                enemy.MagicalAttack,
                enemy.SlashCutRate,
                enemy.BlowCutRate,
                enemy.ThrustCutRate,
                enemy.MagicCutRate,
                enemy.FireCutRate,
                enemy.ThunderCutRate,
                enemy.Speed,
                enemy.Experience,
                enemy.BehaviourPoint,
                enemy.BehaviourPoint,
                enemy.Attribute
            )
        {
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
