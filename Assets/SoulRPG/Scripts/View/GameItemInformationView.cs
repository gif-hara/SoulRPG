using System.Collections.Generic;
using System.Linq;
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
            Thumbnail.sprite = item.Thumbnail;
            Name.text = item.Name;
            foreach (var element in skillElements)
            {
                Object.Destroy(element);
            }
            skillElements.Clear();
            if (item.Id.TryGetMasterDataWeapon(out var weapon))
            {
                SetActiveFromEquipmentType(EquipmentType.Weapon);
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
                    skillElement.Q<TMP_Text>("Name").text = masterDataSkill.Name;
                    skillElement.Q<TMP_Text>("Description").text = masterDataSkill.Description;
                    skillElement
                        .Q<HKUIDocument>("Parameter.BehaviourPoint")
                        .Q<TMP_Text>("Value")
                        .text = masterDataSkill.NeedBehaviourPoint.ToString();
                    skillElement
                        .Q<HKUIDocument>("Parameter.Stamina")
                        .Q<TMP_Text>("Value")
                        .text = masterDataSkill.NeedStamina.ToString();
                    skillElements.Add(skillElement.gameObject);
                }
            }
        }

        private void SetActiveFromEquipmentType(EquipmentType equipmentType)
        {
            document.Q("Statuses.Weapon").SetActive(equipmentType == EquipmentType.Weapon);
            document.Q("Statuses.Armor").SetActive(equipmentType == EquipmentType.Armor);
            document.Q("Statuses.Accessory").SetActive(equipmentType == EquipmentType.Accessory);
        }

        private Image Thumbnail => document.Q<Image>("Thumbnail");

        private TMP_Text Name => document.Q<TMP_Text>("Name");
    }
}
