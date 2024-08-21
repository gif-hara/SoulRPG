using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.CharacterControllers;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameCameraController : MonoBehaviour
    {
        [SerializeField]
        private HKUIDocument document;

        [SerializeField]
        private PostProcessVolume pinchPostProcessVolume;

        public void Setup(Character player)
        {
            TinyServiceLocator.Resolve<GameEvents>().OnBeginBattle
                .Subscribe(x =>
                {
                    x.Player.Events.OnTakeDamage
                        .Subscribe(_ =>
                        {
                            document.Q<SequenceMonobehaviour>("Animation.OnTakeDamage").PlayAsync().Forget();
                        })
                        .RegisterTo(destroyCancellationToken);
                })
                .RegisterTo(destroyCancellationToken);
            var isPinch = player.InstanceStatus.IsPinch;
            Observable.Merge(
                player.InstanceStatus.HitPointAsObservable(),
                player.InstanceStatus.HitPointMaxAsObservable()
            )
            .Subscribe(x =>
            {
                var newIsPinch = player.InstanceStatus.IsPinch;
                if (isPinch != newIsPinch)
                {
                    pinchPostProcessVolume.weight = newIsPinch ? 1.0f : 0.0f;
                    isPinch = newIsPinch;
                }
            })
            .RegisterTo(destroyCancellationToken);
            pinchPostProcessVolume.weight = isPinch ? 1.0f : 0.0f;
        }
    }
}
