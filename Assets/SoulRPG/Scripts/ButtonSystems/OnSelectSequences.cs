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
    public sealed class OnSelectSequences : MonoBehaviour
    {
        [SerializeField]
        private Selectable selectable;

        [SerializeField]
        private SequenceMonobehaviour sequence;

        void Awake()
        {
            selectable.OnSelectAsObservable()
                .Subscribe(_ =>
                {
                    sequence.PlayAsync(destroyCancellationToken).Forget();
                })
                .RegisterTo(destroyCancellationToken);
        }
    }
}
