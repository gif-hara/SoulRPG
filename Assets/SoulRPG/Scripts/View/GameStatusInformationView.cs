using System;
using System.Linq;
using System.Threading;
using HK;
using R3;
using SoulRPG.CharacterControllers;
using TMPro;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameStatusInformationView
    {
        public static void Open(HKUIDocument documentPrefab, Character character, CancellationToken scope)
        {
            var document = UnityEngine.Object.Instantiate(documentPrefab);
            scope.Register(() =>
            {
                if (document != null)
                {
                    UnityEngine.Object.Destroy(document.gameObject);
                }
            });
            SetValue("Level", character.GrowthParameter.Level.ToString());
            SetValue("Experience", character.InstanceStatus.Experience.ToString());
            SetValue("HitPoint", $"{character.InstanceStatus.HitPoint} / {character.InstanceStatus.HitPointMax}");
            SetValue("Stamina", $"{character.InstanceStatus.Stamina} / {character.InstanceStatus.StaminaMax}");
            SetValue("PhysicalAttack", character.GrowthParameter.PhysicalStrength.ToString());
            SetValue("MagicalAttack", character.GrowthParameter.MagicalStrength.ToString());
            SetValue("Speed", (character.GrowthParameter.Speed + character.Equipment.TotalSpeed).ToString());
            SetValue("BehaviourPoint", string.Concat(Enumerable.Repeat("<sprite name=\"BehaviourPoint\">", character.GrowthParameter.BehaviourPoint)));
            SetValue("CutRate.Slash", $"{character.Equipment.TotalSlashCutRate.ToPercentage():0.00}%");
            SetValue("CutRate.Blow", $"{character.Equipment.TotalBlowCutRate.ToPercentage():0.00}%");
            SetValue("CutRate.Thrust", $"{character.Equipment.TotalThrustCutRate.ToPercentage():0.00}%");
            SetValue("CutRate.Fire", $"{character.Equipment.TotalFireCutRate.ToPercentage():0.00}%");
            SetValue("CutRate.Magic", $"{character.Equipment.TotalMagicCutRate.ToPercentage():0.00}%");
            SetValue("CutRate.Thunder", $"{character.Equipment.TotalThunderCutRate.ToPercentage():0.00}%");

            void SetValue(string key, string value)
            {
                document.Q<HKUIDocument>(key).Q<TMP_Text>("Value").text = value;
            }
        }
    }
}
