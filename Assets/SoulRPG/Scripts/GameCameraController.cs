using Cysharp.Threading.Tasks;
using HK;
using R3;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameCameraController : MonoBehaviour
    {
        [SerializeField]
        private HKUIDocument document;

        void Start()
        {
            TinyServiceLocator.Resolve<GameEvents>().OnBeginBattle
                .Subscribe(x =>
                {
                    x.Player.Events.OnTakeDamage
                        .Subscribe(_ =>
                        {
                            document.Q<SequenceMonobehaviour>("Animation.TakeDamage").PlayAsync().Forget();
                        })
                        .RegisterTo(destroyCancellationToken);
                })
                .RegisterTo(destroyCancellationToken);
        }
    }
}
