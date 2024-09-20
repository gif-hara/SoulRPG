using System;
using HK;
using UnitySequencerSystem;

namespace SoulRPG.ContainerEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class IsTrrialVersion : IContainerEvaluator
    {
        public bool Evaluate(Container container)
        {
            return TinyServiceLocator.Resolve<GameRule>().IsTrialVersion;
        }
    }
}
