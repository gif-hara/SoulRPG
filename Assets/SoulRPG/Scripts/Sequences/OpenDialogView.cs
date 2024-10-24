using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using UnityEngine;
using UnitySequencerSystem;
using UnitySequencerSystem.Resolvers;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class OpenDialogView : ISequence
    {
        [SerializeField]
        private HKUIDocument documentPrefab;

        [SerializeReference, SubclassSelector]
        private StringResolver titleResolver;

        [SerializeField]
        private List<Element> elements;

        public async UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            await UniTask.NextFrame(cancellationToken);
            var title = titleResolver.Resolve(container).Localized();
            var saveData = SaveData.LoadSafe();
            title = title.Replace("{SealedName}", saveData.playerData.sealedName);
            title = title.Replace("{PlayerName}", saveData.playerData.name);
            var result = await DialogView.ConfirmAsync(documentPrefab, title, elements.ConvertAll(e => e.messageResolver.Resolve(container).Localized()), 0, cancellationToken);
            var sequencer = new Sequencer(container, elements[result].nextResolver.Resolve(container));
            await sequencer.PlayAsync(cancellationToken);
        }

        [Serializable]
        public class Element
        {
            [SerializeReference, SubclassSelector]
            public StringResolver messageResolver;

            [SerializeReference, SubclassSelector]
            public SequencesResolver nextResolver;
        }
    }
}
