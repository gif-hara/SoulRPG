using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnitySequencerSystem;
using UnitySequencerSystem.Resolvers;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class For : ISequence
    {
        [SerializeReference, SubclassSelector]
        private IntResolver countResolver;

        [SerializeReference, SubclassSelector]
        private SequencesResolver sequencesResolver;

        public async UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var count = countResolver.Resolve(container);
            var sequences = sequencesResolver.Resolve(container);
            for (var i = 0; i < count; i++)
            {
                await new Sequencer(container, sequences).PlayAsync(cancellationToken);
            }
        }
    }
}
