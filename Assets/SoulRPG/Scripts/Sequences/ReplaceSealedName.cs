using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class ReplaceSealedName : ISequence
    {
        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var saveData = SaveData.LoadSafe();
            saveData.playerData.sealedName = saveData.playerData.name;
            saveData.playerData.name = "";
            saveData.Save();
            return UniTask.CompletedTask;
        }
    }
}
