using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameItemInformationView
    {
        private readonly HKUIDocument document;

        private readonly List<GameObject> skillElements = new();

        private enum EquipmentType
        {
            Weapon,
            Armor,
            Accessory,
        }

        public GameItemInformationView(HKUIDocument documentPrefab, CancellationToken scope)
        {
            document = Object.Instantiate(documentPrefab);
            scope.Register(() =>
            {
                if (document == null)
                {
                    return;
                }
                Object.Destroy(document.gameObject);
            });
        }

        public void Setup(MasterData.Item item)
        {
            SetActiveEmptyArea(false);
            Thumbnail.sprite = item.Thumbnail;
            Name.text = item.Name.Localized();
            Description.text = item.Description.Localized();
            foreach (var element in skillElements)
            {
                Object.Destroy(element);
            }
            skillElements.Clear();
            if (item.Id.TryGetMasterDataWeapon(out var weapon))
            {
                SetActiveFromEquipmentType(EquipmentType.Weapon);
                document
                    .Q<HKUIDocument>("Weapon.Attribute")
                    .Q<TMP_Text>("Value")
                    .text = weapon.AttackAttribute.ToUIString();
                document
                    .Q<HKUIDocument>("Weapon.Strength")
                    .Q<TMP_Text>("Value")
                    .text = weapon.Strength.ToString();
                document
                    .Q<HKUIDocument>("Weapon.Speed")
                    .Q<TMP_Text>("Value")
                    .text = weapon.Speed.ToString();
                var skillParent = document.Q<Transform>("Parent.Skill");
                var skillElementPrefab = document.Q<HKUIDocument>("Prefab.Skill");
                foreach (var i in weapon.Skills.Where(x => x.ContainsMasterDataSkill()))
                {
                    var masterDataSkill = i.GetMasterDataSkill();
                    var skillElement = Object.Instantiate(skillElementPrefab, skillParent);
                    skillElement.Q<TMP_Text>("Name").text = masterDataSkill.Name.Localized();
                    var description = masterDataSkill.Description.Localized();
                    foreach (var s in masterDataSkill.AdditionalDescriptions)
                    {
                        description += $"\n<size=80%><color=#FFDDDD>{s.CreateAdditionalDescription()}</color></size>";
                    }
                    skillElement.Q<TMP_Text>("Description").text = description;
                    var sb = new StringBuilder();
                    for (var bp = 0; bp < masterDataSkill.NeedBehaviourPoint; bp++)
                    {
                        sb.Append("<sprite name=\"BehaviourPoint\">");
                    }
                    skillElement
                        .Q<HKUIDocument>("Parameter.BehaviourPoint")
                        .Q<TMP_Text>("Value")
                        .text = sb.ToString();
                    skillElement
                        .Q<HKUIDocument>("Parameter.Stamina")
                        .Q<TMP_Text>("Value")
                        .text = masterDataSkill.NeedStamina.ToString();
                    skillElements.Add(skillElement.gameObject);
                }
            }
            else if (item.Id.TryGetMasterDataArmorHead(out var armor))
            {
                SetupAsArmor(armor);
            }
            else if (item.Id.TryGetMasterDataArmorBody(out armor))
            {
                SetupAsArmor(armor);
            }
            else if (item.Id.TryGetMasterDataArmorArms(out armor))
            {
                SetupAsArmor(armor);
            }
            else if (item.Id.TryGetMasterDataArmorLegs(out armor))
            {
                SetupAsArmor(armor);
            }
            else if (item.Id.TryGetMasterDataAccessory(out var accessory))
            {
                SetupAccessory(accessory);
            }
        }

        public void SetupAsEmpty()
        {
            SetActiveEmptyArea(true);
        }

        private void SetActiveEmptyArea(bool isEmpty)
        {
            document.Q("Area.Empty").SetActive(isEmpty);
            document.Q("Viewport").SetActive(!isEmpty);
        }

        private void SetupAsArmor(MasterData.Armor armor)
        {
            SetActiveFromEquipmentType(EquipmentType.Armor);
            document.Q<HKUIDocument>("Armor.Speed").Q<TMP_Text>("Value").text = armor.Speed.ToString();
            document.Q<HKUIDocument>("Armor.SlashCutRate").Q<TMP_Text>("Value").text = $"{armor.SlashCutRate.ToPercentage():0.00}%";
            document.Q<HKUIDocument>("Armor.BlowCutRate").Q<TMP_Text>("Value").text = $"{armor.BlowCutRate.ToPercentage():0.00}%";
            document.Q<HKUIDocument>("Armor.ThrustCutRate").Q<TMP_Text>("Value").text = $"{armor.ThrustCutRate.ToPercentage():0.00}%";
            document.Q<HKUIDocument>("Armor.MagicCutRate").Q<TMP_Text>("Value").text = $"{armor.MagicCutRate.ToPercentage():0.00}%";
            document.Q<HKUIDocument>("Armor.FireCutRate").Q<TMP_Text>("Value").text = $"{armor.FireCutRate.ToPercentage():0.00}%";
            document.Q<HKUIDocument>("Armor.ThunderCutRate").Q<TMP_Text>("Value").text = $"{armor.ThunderCutRate.ToPercentage():0.00}%";
        }

        private void SetupAccessory(MasterData.Accessory accessory)
        {
            SetActiveFromEquipmentType(EquipmentType.Accessory);
            document.Q<HKUIDocument>("Accessory.Vitality").Q<TMP_Text>("Value").text = accessory.Vitality.ToString();
            document.Q<HKUIDocument>("Accessory.Stamina").Q<TMP_Text>("Value").text = accessory.Stamina.ToString();
            document.Q<HKUIDocument>("Accessory.PhysicalAttack").Q<TMP_Text>("Value").text = accessory.PhysicalAttack.ToString();
            document.Q<HKUIDocument>("Accessory.MagicalAttack").Q<TMP_Text>("Value").text = accessory.MagicalAttack.ToString();
            document.Q<HKUIDocument>("Accessory.Speed").Q<TMP_Text>("Value").text = accessory.Speed.ToString();
        }

        private void SetActiveFromEquipmentType(EquipmentType equipmentType)
        {
            document.Q("Statuses.Weapon").SetActive(equipmentType == EquipmentType.Weapon);
            document.Q("Statuses.Armor").SetActive(equipmentType == EquipmentType.Armor);
            document.Q("Statuses.Accessory").SetActive(equipmentType == EquipmentType.Accessory);
        }

        private Image Thumbnail => document.Q<Image>("Thumbnail");

        private TMP_Text Name => document.Q<TMP_Text>("Name");

        private TMP_Text Description => document.Q<TMP_Text>("Description");
    }
}
