using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using TMPro;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class BattleInformationEnemyView
    {
        private HKUIDocument document;

        public async UniTask OpenAsync(HKUIDocument documentPrefab, BattleCharacter enemy, CancellationToken scope)
        {
            document = Object.Instantiate(documentPrefab);
            var hitPoint = document.Q<HKUIDocument>("HitPoint");
            var cutRateSlash = document.Q<HKUIDocument>("CutRate.Slash");
            var cutRateBlow = document.Q<HKUIDocument>("CutRate.Blow");
            var cutRateThrust = document.Q<HKUIDocument>("CutRate.Thrust");
            var cutRateMagic = document.Q<HKUIDocument>("CutRate.Magic");
            var cutRateFire = document.Q<HKUIDocument>("CutRate.Fire");
            var cutRateThunder = document.Q<HKUIDocument>("CutRate.Thunder");
            document.gameObject.SetActive(false);
            document.Q<HKUIDocument>("Name").Q<TMP_Text>("Value").text = enemy.BattleStatus.Name;
            Observable.Merge(
                enemy.BattleStatus.HitPointReactiveProperty,
                enemy.BattleStatus.HitPointMaxReactiveProperty
            )
            .Subscribe(_ =>
            {
                hitPoint.Q<TMP_Text>("Value").text = $"{enemy.BattleStatus.HitPoint}/{enemy.BattleStatus.HitPointMax}";
            })
            .RegisterTo(scope);
            Observable.Merge(
                enemy.BattleStatus.SlashCutRateReactiveProperty,
                enemy.BattleStatus.BlowCutRateReactiveProperty,
                enemy.BattleStatus.ThrustCutRateReactiveProperty,
                enemy.BattleStatus.MagicCutRateReactiveProperty,
                enemy.BattleStatus.FireCutRateReactiveProperty,
                enemy.BattleStatus.ThunderCutRateReactiveProperty
            )
            .Subscribe(_ =>
            {
                cutRateSlash.Q<TMP_Text>("Value").text = $"{enemy.BattleStatus.SlashCutRate}";
                cutRateBlow.Q<TMP_Text>("Value").text = $"{enemy.BattleStatus.BlowCutRate}";
                cutRateThrust.Q<TMP_Text>("Value").text = $"{enemy.BattleStatus.ThrustCutRate}";
                cutRateMagic.Q<TMP_Text>("Value").text = $"{enemy.BattleStatus.MagicCutRate}";
                cutRateFire.Q<TMP_Text>("Value").text = $"{enemy.BattleStatus.FireCutRate}";
                cutRateThunder.Q<TMP_Text>("Value").text = $"{enemy.BattleStatus.ThunderCutRate}";
            })
            .RegisterTo(scope);
            await UniTask.WaitUntilCanceled(scope);

            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }
        }

        public void SetActive(bool value)
        {
            document.gameObject.SetActive(value);
        }
    }
}
