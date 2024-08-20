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

        public string NameWithTag => AllyType.WithTag(Name);

        private readonly ReactiveProperty<int> hitPointMaxReactiveProperty;
        public ReadOnlyReactiveProperty<int> HitPointMaxReactiveProperty => hitPointMaxReactiveProperty;
        public int HitPointMax => hitPointMaxReactiveProperty.Value;

        private readonly ReactiveProperty<int> hitPointReactiveProperty;
        public ReadOnlyReactiveProperty<int> HitPointReactiveProperty => hitPointReactiveProperty;
        public int HitPoint => hitPointReactiveProperty.Value;

        private readonly ReactiveProperty<int> guardPointReactiveProperty;
        public ReadOnlyReactiveProperty<int> GuardPointReactiveProperty => guardPointReactiveProperty;
        public int GuardPoint => guardPointReactiveProperty.Value;

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

        public Define.AllyType AllyType { get; }

        private readonly ReactiveProperty<int> magicCountReactiveProperty = new(0);
        public ReadOnlyReactiveProperty<int> MagicCountReactiveProperty => magicCountReactiveProperty;
        public int MagicCount => magicCountReactiveProperty.Value;

        private readonly ReactiveProperty<int> knifeCountReactiveProperty = new(0);
        public ReadOnlyReactiveProperty<int> KnifeCountReactiveProperty => knifeCountReactiveProperty;
        public int KnifeCount => knifeCountReactiveProperty.Value;

        public bool IsDead => hitPointReactiveProperty.Value <= 0;

        public float HitPointRate => (float)hitPointReactiveProperty.Value / hitPointMaxReactiveProperty.Value;

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
            Define.CharacterAttribute attribute,
            Define.AllyType allyType
        )
        {
            Name = name;
            hitPointMaxReactiveProperty = new ReactiveProperty<int>(hitPointMax);
            hitPointReactiveProperty = new ReactiveProperty<int>(hitPoint);
            guardPointReactiveProperty = new ReactiveProperty<int>(0);
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
            this.AllyType = allyType;
#if DEBUG
            AddDebugPanel();
#endif
        }

        public CharacterBattleStatus(Character character, Define.AllyType allyType)
            : this
            (
                character.Name,
                character.InstanceStatus.HitPointMax,
                character.InstanceStatus.HitPoint,
                character.InstanceStatus.StaminaMax,
                character.InstanceStatus.Stamina,
                character.GrowthParameter.PhysicalStrength + character.GrowthParameter.Level + character.Equipment.TotalPhysicalAttack,
                character.GrowthParameter.MagicalStrength + character.GrowthParameter.Level + character.Equipment.TotalMagicalAttack,
                character.Equipment.TotalSlashCutRate,
                character.Equipment.TotalBlowCutRate,
                character.Equipment.TotalThrustCutRate,
                character.Equipment.TotalMagicCutRate,
                character.Equipment.TotalFireCutRate,
                character.Equipment.TotalThunderCutRate,
                character.GrowthParameter.Speed + character.Equipment.TotalSpeed + character.GrowthParameter.Level + character.Equipment.TotalSpeed,
                0,
                TinyServiceLocator.Resolve<GameRule>().BehaviourPointMax,
                0,
                character.Attribute,
                allyType
            )
        {
            hitPointReactiveProperty
                .Subscribe(x =>
                {
                    character.InstanceStatus.SetHitPoint(x);
                })
                .RegisterTo(scope.Token);
            guardPointReactiveProperty
                .Subscribe(x =>
                {
                    character.InstanceStatus.SetGuardPoint(x);
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
                0,
                blueprint.Attribute,
                blueprint.AllyType
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
                0,
                enemy.Attribute,
                Define.AllyType.Enemy
            )
        {
        }

        public void TakeDamage(int damage)
        {
            var guardPointDamage = Mathf.Min(guardPointReactiveProperty.Value, damage);
            guardPointReactiveProperty.Value -= guardPointDamage;
            damage -= guardPointDamage;
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

        public void AddBehaviourPoint(int value)
        {
            behaviourPointReactiveProperty.Value += value;
        }

        public void RecoveryBehaviourPoint()
        {
            behaviourPointReactiveProperty.Value += behaviourPointMaxReactiveProperty.Value;
        }

        public bool CanBehaviour()
        {
            return behaviourPointReactiveProperty.Value > 0;
        }

        public void AddGuardPoint(int value)
        {
            guardPointReactiveProperty.Value = Mathf.Clamp(guardPointReactiveProperty.Value + value, 0, HitPointMax / 2);
        }

        public void ResetGuardPoint()
        {
            guardPointReactiveProperty.Value = 0;
        }

        public void AddStamina(int value)
        {
            staminaReactiveProperty.Value = Mathf.Clamp(staminaReactiveProperty.Value + value, 0, staminaMaxReactiveProperty.Value);
        }

        public bool HasBehaviourPoint(int needBehaviourPoint)
        {
            return behaviourPointReactiveProperty.Value >= needBehaviourPoint;
        }

        public bool HasStamina(int needStamina)
        {
            return staminaReactiveProperty.Value >= needStamina;
        }

        public void AddMagicCount(int value)
        {
            magicCountReactiveProperty.Value = Mathf.Clamp(
                magicCountReactiveProperty.Value + value,
                0,
                TinyServiceLocator.Resolve<GameRule>().MagicCountMax
            );
        }

        public void AddKnifeCount(int value)
        {
            knifeCountReactiveProperty.Value = Mathf.Clamp(
                knifeCountReactiveProperty.Value + value,
                0,
                TinyServiceLocator.Resolve<GameRule>().KnifeCountMax
            );
        }

        public void ResetMagicCount()
        {
            magicCountReactiveProperty.Value = 0;
        }

        public void ResetKnifeCount()
        {
            knifeCountReactiveProperty.Value = 0;
        }

        public void RecoveryHitPoint(int value)
        {
            hitPointReactiveProperty.Value = Mathf.Min(hitPointMaxReactiveProperty.Value, hitPointReactiveProperty.Value + value);
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
