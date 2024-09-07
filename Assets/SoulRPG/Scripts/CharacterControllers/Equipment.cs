using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.CharacterControllers;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Equipment
    {
        private readonly Character character;

        private readonly List<ReactiveProperty<int>> weaponIds = new();
        public ReadOnlyReactiveProperty<int> WeaponIdAsObservable(int index) => weaponIds[index];
        public int GetWeaponId(int index) => weaponIds[index].Value;
        public IEnumerable<int> GetWeaponIds() => weaponIds.Select(x => x.Value);

        private readonly ReactiveProperty<int> headId = new();
        public ReadOnlyReactiveProperty<int> HeadIdAsObservable() => headId;
        public int HeadId => headId.Value;

        private readonly ReactiveProperty<int> bodyId = new();
        public ReadOnlyReactiveProperty<int> BodyIdAsObservable() => bodyId;
        public int BodyId => bodyId.Value;

        private readonly ReactiveProperty<int> armId = new();
        public ReadOnlyReactiveProperty<int> ArmIdAsObservable() => armId;
        public int ArmId => armId.Value;

        private readonly ReactiveProperty<int> legId = new();
        public ReadOnlyReactiveProperty<int> LegIdAsObservable() => legId;
        public int LegId => legId.Value;

        private readonly List<ReactiveProperty<int>> accessoryIds = new();
        public ReadOnlyReactiveProperty<int> AccessoryIdAsObservable(int index) => accessoryIds[index];
        public int GetAccessoryId(int index) => accessoryIds[index].Value;
        public IEnumerable<int> GetAccessoryIds() => accessoryIds.Select(x => x.Value);

        public int TotalVitality
        {
            get
            {
                var result = 0;
                foreach (var i in accessoryIds)
                {
                    if (i.Value.TryGetMasterDataAccessory(out var accessory))
                    {
                        result += accessory.Vitality;
                    }
                }
                return result;
            }
        }

        public int TotalStamina
        {
            get
            {
                var result = 0;
                foreach (var i in accessoryIds)
                {
                    if (i.Value.TryGetMasterDataAccessory(out var accessory))
                    {
                        result += accessory.Stamina;
                    }
                }
                return result;
            }
        }

        public int TotalPhysicalAttack
        {
            get
            {
                var result = 0;
                foreach (var i in accessoryIds)
                {
                    if (i.Value.TryGetMasterDataAccessory(out var accessory))
                    {
                        result += accessory.PhysicalAttack;
                    }
                }
                return result;
            }
        }

        public int TotalMagicalAttack
        {
            get
            {
                var result = 0;
                foreach (var i in accessoryIds)
                {
                    if (i.Value.TryGetMasterDataAccessory(out var accessory))
                    {
                        result += accessory.MagicalAttack;
                    }
                }
                return result;
            }
        }

        public float TotalSlashCutRate
        {
            get
            {
                var result = 0.0f;
                if (HeadId.TryGetMasterDataArmorHead(out var armorHead))
                {
                    result += armorHead.SlashCutRate;
                }
                if (BodyId.TryGetMasterDataArmorBody(out var armorBody))
                {
                    result += armorBody.SlashCutRate;
                }
                if (ArmId.TryGetMasterDataArmorArms(out var armorArm))
                {
                    result += armorArm.SlashCutRate;
                }
                if (LegId.TryGetMasterDataArmorLegs(out var armorLeg))
                {
                    result += armorLeg.SlashCutRate;
                }
                return result;
            }
        }

        public float TotalBlowCutRate
        {
            get
            {
                var result = 0.0f;
                if (HeadId.TryGetMasterDataArmorHead(out var armorHead))
                {
                    result += armorHead.BlowCutRate;
                }
                if (BodyId.TryGetMasterDataArmorBody(out var armorBody))
                {
                    result += armorBody.BlowCutRate;
                }
                if (ArmId.TryGetMasterDataArmorArms(out var armorArm))
                {
                    result += armorArm.BlowCutRate;
                }
                if (LegId.TryGetMasterDataArmorLegs(out var armorLeg))
                {
                    result += armorLeg.BlowCutRate;
                }
                return result;
            }
        }

        public float TotalThrustCutRate
        {
            get
            {
                var result = 0.0f;
                if (HeadId.TryGetMasterDataArmorHead(out var armorHead))
                {
                    result += armorHead.ThrustCutRate;
                }
                if (BodyId.TryGetMasterDataArmorBody(out var armorBody))
                {
                    result += armorBody.ThrustCutRate;
                }
                if (ArmId.TryGetMasterDataArmorArms(out var armorArm))
                {
                    result += armorArm.ThrustCutRate;
                }
                if (LegId.TryGetMasterDataArmorLegs(out var armorLeg))
                {
                    result += armorLeg.ThrustCutRate;
                }
                return result;
            }
        }

        public float TotalMagicCutRate
        {
            get
            {
                var result = 0.0f;
                if (HeadId.TryGetMasterDataArmorHead(out var armorHead))
                {
                    result += armorHead.MagicCutRate;
                }
                if (BodyId.TryGetMasterDataArmorBody(out var armorBody))
                {
                    result += armorBody.MagicCutRate;
                }
                if (ArmId.TryGetMasterDataArmorArms(out var armorArm))
                {
                    result += armorArm.MagicCutRate;
                }
                if (LegId.TryGetMasterDataArmorLegs(out var armorLeg))
                {
                    result += armorLeg.MagicCutRate;
                }
                return result;
            }
        }

        public float TotalFireCutRate
        {
            get
            {
                var result = 0.0f;
                if (HeadId.TryGetMasterDataArmorHead(out var armorHead))
                {
                    result += armorHead.FireCutRate;
                }
                if (BodyId.TryGetMasterDataArmorBody(out var armorBody))
                {
                    result += armorBody.FireCutRate;
                }
                if (ArmId.TryGetMasterDataArmorArms(out var armorArm))
                {
                    result += armorArm.FireCutRate;
                }
                if (LegId.TryGetMasterDataArmorLegs(out var armorLeg))
                {
                    result += armorLeg.FireCutRate;
                }
                return result;
            }
        }

        public float TotalThunderCutRate
        {
            get
            {
                var result = 0.0f;
                if (HeadId.TryGetMasterDataArmorHead(out var armorHead))
                {
                    result += armorHead.ThunderCutRate;
                }
                if (BodyId.TryGetMasterDataArmorBody(out var armorBody))
                {
                    result += armorBody.ThunderCutRate;
                }
                if (ArmId.TryGetMasterDataArmorArms(out var armorArm))
                {
                    result += armorArm.ThunderCutRate;
                }
                if (LegId.TryGetMasterDataArmorLegs(out var armorLeg))
                {
                    result += armorLeg.ThunderCutRate;
                }
                return result;
            }
        }

        public int TotalSpeed
        {
            get
            {
                var result = 0;
                foreach (var i in weaponIds)
                {
                    if (i.Value.TryGetMasterDataWeapon(out var weapon))
                    {
                        result += weapon.Speed;
                    }
                }
                if (HeadId.TryGetMasterDataArmorHead(out var armorHead))
                {
                    result += armorHead.Speed;
                }
                if (BodyId.TryGetMasterDataArmorBody(out var armorBody))
                {
                    result += armorBody.Speed;
                }
                if (ArmId.TryGetMasterDataArmorArms(out var armorArm))
                {
                    result += armorArm.Speed;
                }
                if (LegId.TryGetMasterDataArmorLegs(out var armorLeg))
                {
                    result += armorLeg.Speed;
                }
                foreach (var i in accessoryIds)
                {
                    if (i.Value.TryGetMasterDataAccessory(out var accessory))
                    {
                        result += accessory.Speed;
                    }
                }
                return result;
            }
        }

        public Equipment()
        {
            for (var i = 0; i < 4; i++)
            {
                weaponIds.Add(new ReactiveProperty<int>());
            }

            for (var i = 0; i < 4; i++)
            {
                accessoryIds.Add(new ReactiveProperty<int>());
            }
        }

        public Equipment(Character character, EquipmentBlueprint blueprint)
        {
            this.character = character;
            for (var i = 0; i < 4; i++)
            {
                weaponIds.Add(new ReactiveProperty<int>(blueprint.WeaponIds[i]));
            }

            headId = new ReactiveProperty<int>(blueprint.HeadId);
            bodyId = new ReactiveProperty<int>(blueprint.BodyId);
            armId = new ReactiveProperty<int>(blueprint.ArmId);
            legId = new ReactiveProperty<int>(blueprint.LegId);

            for (var i = 0; i < 4; i++)
            {
                accessoryIds.Add(new ReactiveProperty<int>(blueprint.AccessoryIds[i]));
            }
        }

        public void BeginObserve(Character character)
        {
            character.Events.OnAcquiredItem
                .Subscribe(x =>
                {
                    var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
                    if (x.itemId.TryGetMasterDataWeapon(out var masterDataWeapon))
                    {
                        foreach (var i in weaponIds)
                        {
                            if (i.Value == 0)
                            {
                                i.Value = x.itemId;
                                gameEvents.OnRequestShowMessage.OnNext(new("使えそうだ。空いてる武器枠に装備した。"));
                                character.Events.OnChangedEquipment.OnNext(Unit.Default);
                                break;
                            }
                        }
                    }
                    else if (x.itemId.TryGetMasterDataArmorHead(out var masterDataArmorHead) && headId.Value == 0)
                    {
                        headId.Value = x.itemId;
                        gameEvents.OnRequestShowMessage.OnNext(new("使えそうだ。頭に装備した。"));
                        character.Events.OnChangedEquipment.OnNext(Unit.Default);
                    }
                    else if (x.itemId.TryGetMasterDataArmorBody(out var masterDataArmorBody) && bodyId.Value == 0)
                    {
                        bodyId.Value = x.itemId;
                        gameEvents.OnRequestShowMessage.OnNext(new("使えそうだ。胴に装備した。"));
                        character.Events.OnChangedEquipment.OnNext(Unit.Default);
                    }
                    else if (x.itemId.TryGetMasterDataArmorArms(out var masterDataArmorArms) && armId.Value == 0)
                    {
                        armId.Value = x.itemId;
                        gameEvents.OnRequestShowMessage.OnNext(new("使えそうだ。腕に装備した。"));
                        character.Events.OnChangedEquipment.OnNext(Unit.Default);
                    }
                    else if (x.itemId.TryGetMasterDataArmorLegs(out var masterDataArmorLegs) && legId.Value == 0)
                    {
                        legId.Value = x.itemId;
                        gameEvents.OnRequestShowMessage.OnNext(new("使えそうだ。脚に装備した。"));
                        character.Events.OnChangedEquipment.OnNext(Unit.Default);
                    }
                    else if (x.itemId.TryGetMasterDataAccessory(out var masterDataAccessory))
                    {
                        foreach (var i in accessoryIds)
                        {
                            if (i.Value == 0)
                            {
                                i.Value = x.itemId;
                                gameEvents.OnRequestShowMessage.OnNext(new("使えそうだ。空いてるアクセサリー枠に装備した。"));
                                character.Events.OnChangedEquipment.OnNext(Unit.Default);
                                break;
                            }
                        }
                    }
                })
                .RegisterTo(character.LifeScope);
        }

        public void EquipWeapon(int index, int weaponId)
        {
            foreach (var i in weaponIds)
            {
                // 既に装備している場合は入れ替える
                if (i.Value == weaponId)
                {
                    i.Value = weaponIds[index].Value;
                    break;
                }
            }
            weaponIds[index].Value = weaponId;
            character.Events.OnChangedEquipment.OnNext(Unit.Default);
        }

        public void EquipHead(int headId)
        {
            if (this.headId.Value == headId)
            {
                this.headId.Value = 0;
            }
            else
            {
                this.headId.Value = headId;
            }
            character.Events.OnChangedEquipment.OnNext(Unit.Default);
        }

        public void EquipBody(int bodyId)
        {
            if (this.bodyId.Value == bodyId)
            {
                this.bodyId.Value = 0;
            }
            else
            {
                this.bodyId.Value = bodyId;
            }
            character.Events.OnChangedEquipment.OnNext(Unit.Default);
        }

        public void EquipArms(int armId)
        {
            if (this.armId.Value == armId)
            {
                this.armId.Value = 0;
            }
            else
            {
                this.armId.Value = armId;
            }
            character.Events.OnChangedEquipment.OnNext(Unit.Default);
        }

        public void EquipLegs(int legId)
        {
            if (this.legId.Value == legId)
            {
                this.legId.Value = 0;
            }
            else
            {
                this.legId.Value = legId;
            }
            character.Events.OnChangedEquipment.OnNext(Unit.Default);
        }

        public void EquipAccessory(int index, int accessoryId)
        {
            foreach (var i in accessoryIds)
            {
                // 既に装備している場合は入れ替える
                if (i.Value == accessoryId)
                {
                    i.Value = accessoryIds[index].Value;
                    break;
                }
            }
            accessoryIds[index].Value = accessoryId;
            character.Events.OnChangedEquipment.OnNext(Unit.Default);
        }

        public async UniTask BeginBattleAsync(BattleCharacter actor, BattleCharacter target, CancellationToken scope)
        {
            foreach (var i in accessoryIds)
            {
                if (i.Value.TryGetMasterDataAccessory(out var accessory))
                {
                    var container = new Container();
                    container.Register("Actor", actor);
                    container.Register("Target", target);
                    await new Sequencer(container, accessory.BeginBattleSequences.Sequences).PlayAsync(scope);
                }
            }
        }

        public void Sync(EquipmentBlueprint other)
        {
            for (var i = 0; i < 4; i++)
            {
                weaponIds[i].Value = other.WeaponIds[i];
            }

            headId.Value = other.HeadId;
            bodyId.Value = other.BodyId;
            armId.Value = other.ArmId;
            legId.Value = other.LegId;

            for (var i = 0; i < 4; i++)
            {
                accessoryIds[i].Value = other.AccessoryIds[i];
            }
        }

        public void Clear()
        {
            for (var i = 0; i < 4; i++)
            {
                weaponIds[i].Value = 0;
            }

            headId.Value = 0;
            bodyId.Value = 0;
            armId.Value = 0;
            legId.Value = 0;

            for (var i = 0; i < 4; i++)
            {
                accessoryIds[i].Value = 0;
            }
        }

        public SaveData.EquipmentData CreateSaveData()
        {
            return new()
            {
                weaponIds = weaponIds.Select(x => x.Value).ToArray(),
                headId = headId.Value,
                bodyId = bodyId.Value,
                armId = armId.Value,
                legId = legId.Value,
                accessoryIds = accessoryIds.Select(x => x.Value).ToArray(),
            };
        }

        public void SyncFromSaveData(SaveData.EquipmentData equipmentData)
        {
            for (var i = 0; i < 4; i++)
            {
                weaponIds[i].Value = equipmentData.weaponIds[i];
            }

            headId.Value = equipmentData.headId;
            bodyId.Value = equipmentData.bodyId;
            armId.Value = equipmentData.armId;
            legId.Value = equipmentData.legId;

            for (var i = 0; i < 4; i++)
            {
                accessoryIds[i].Value = equipmentData.accessoryIds[i];
            }
        }
    }
}
