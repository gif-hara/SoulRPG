using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class AilmentEffectView
    {
        public static async UniTask OpenAsync(HKUIDocument documentPrefab, BattleCharacter battleCharacter, CancellationToken scope)
        {
            var document = Object.Instantiate(documentPrefab);
            battleCharacter.Events.OnAddAilment
                .Subscribe(document, (x, _document) =>
                {
                    var name = !x.IsDebuff ? "PowerUp" : "PowerDown";
                    _document.Q<ParticleSystem>(name).Play();
                });
            await scope.WaitUntilCanceled();
            Object.Destroy(document.gameObject);
        }
    }
}
