using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class OnDeselectSequences : MonoBehaviour
    {
        [SerializeField]
        private Selectable selectable;

        [SerializeField]
        private SequenceMonobehaviour sequence;

        void Awake()
        {
            selectable.OnDeselectAsObservable()
                .Subscribe(_ =>
                {
                    sequence.PlayAsync(destroyCancellationToken).Forget();
                })
                .RegisterTo(destroyCancellationToken);
        }
    }
}