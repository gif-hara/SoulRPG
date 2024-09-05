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
            document.Q<TMP_Text>("Name").text = enemy.BattleStatus.Name;
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
                cutRateSlash.Q<TMP_Text>("Value").text = $"{enemy.BattleStatus.SlashCutRate.ToPercentage():0.00}%";
                cutRateBlow.Q<TMP_Text>("Value").text = $"{enemy.BattleStatus.BlowCutRate.ToPercentage():0.00}%";
                cutRateThrust.Q<TMP_Text>("Value").text = $"{enemy.BattleStatus.ThrustCutRate.ToPercentage():0.00}%";
                cutRateMagic.Q<TMP_Text>("Value").text = $"{enemy.BattleStatus.MagicCutRate.ToPercentage():0.00}%";
                cutRateFire.Q<TMP_Text>("Value").text = $"{enemy.BattleStatus.FireCutRate.ToPercentage():0.00}%";
                cutRateThunder.Q<TMP_Text>("Value").text = $"{enemy.BattleStatus.ThunderCutRate.ToPercentage():0.00}%";
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
