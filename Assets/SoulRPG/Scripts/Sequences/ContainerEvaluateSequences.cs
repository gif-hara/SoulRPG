using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoulRPG.ContainerEvaluators;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class ContainerEvaluateSequences : ISequence
    {
        [SerializeReference, SubclassSelector]
        private IContainerEvaluator containerEvaluator;

        [SerializeReference, SubclassSelector]
        private List<ISequence> sequences;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            if (containerEvaluator.Evaluate(container))
            {
                var sequencer = new Sequencer(container, sequences);
                return sequencer.PlayAsync(cancellationToken);
            }
            return UniTask.CompletedTask;
        }
    }
}
