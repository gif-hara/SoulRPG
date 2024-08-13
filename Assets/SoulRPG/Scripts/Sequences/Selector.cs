using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoulRPG.ContainerEvaluators;
using UnityEngine;
using UnityEngine.Assertions;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class Selector : ISequence
    {
        [SerializeField]
        private List<Element> elements;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            foreach (var element in elements)
            {
                if (element.Evaluator.Evaluate(container))
                {
                    return new Sequencer(container, element.Sequences.Sequences).PlayAsync(cancellationToken);
                }
            }
            Assert.IsTrue(false, "Not found any element");
            return UniTask.CompletedTask;
        }

        [Serializable]
        public sealed class Element
        {
            [SerializeReference, SubclassSelector]
            private IContainerEvaluator evaluator;
            public IContainerEvaluator Evaluator => evaluator;

            [SerializeField]
            private ScriptableSequences sequences;
            public ScriptableSequences Sequences => sequences;
        }
    }
}
