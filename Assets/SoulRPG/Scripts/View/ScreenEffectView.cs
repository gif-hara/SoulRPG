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
        private readonly HKUIDocument document;

        public AilmentEffectView(HKUIDocument documentPrefab, CancellationToken scope)
        {
            document = Object.Instantiate(documentPrefab);
            scope.Register(() => Object.Destroy(document.gameObject));
        }

        public void Subscribe(BattleCharacter battleCharacter, CancellationToken scope)
        {
            battleCharacter.Events.OnAddAilment
                .Subscribe(document, (x, _document) =>
                {
                    var name = !x.IsDebuff ? "PowerUp" : "PowerDown";
                    _document.Q<ParticleSystem>(name).Play();
                })
                .RegisterTo(scope);
        }
    }
}
