using System.Threading;
using HK;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class BattleInformationWeaponView
    {
        private readonly HKUIDocument document;

        public BattleInformationWeaponView(HKUIDocument documentPrefab, CancellationToken scope)
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

        public void Setup(int weaponId)
        {
            if (!weaponId.TryGetMasterDataWeapon(out var weapon))
            {
                weapon = Define.HandWeaponId.GetMasterDataWeapon();
            }
            document.Q<Image>("Thumbnail").sprite = weapon.ItemId.GetMasterDataItem().Thumbnail;
            document.Q<TMP_Text>("Name").text = weapon.ItemId.GetMasterDataItem().Name;
            document.Q<HKUIDocument>("Strength").Q<TMP_Text>("Value").text = weapon.Strength.ToString();
            document.Q<HKUIDocument>("AttackAttribute").Q<TMP_Text>("Value").text = weapon.AttackAttribute.ToUIString();
        }
    }
}
