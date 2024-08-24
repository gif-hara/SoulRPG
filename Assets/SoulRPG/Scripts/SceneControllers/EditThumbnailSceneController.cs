using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class EditThumbnailSceneController : MonoBehaviour
    {
        [SerializeField]
        private SequenceMonobehaviour sequenceMonobehaviour;

        void Start()
        {
            sequenceMonobehaviour.PlayAsync().Forget();
        }
    }
}
