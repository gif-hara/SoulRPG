using System;
using HK;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [CreateAssetMenu(menuName = "SoulRPG/AilmentSequences")]
    public sealed class AilmentSequences : ScriptableObject
    {
        [SerializeField]
        private Element.DictionaryList elements;

        public ScriptableSequences GetSequences(Define.AilmentBehaviourType type)
        {
            return elements.TryGetValue(type, out var element) ? element.Sequences : null;
        }

        [Serializable]
        public sealed class Element
        {
            [SerializeField]
            private Define.AilmentBehaviourType type;

            [SerializeField]
            private ScriptableSequences sequences;
            public ScriptableSequences Sequences => sequences;

            [Serializable]
            public sealed class DictionaryList : DictionaryList<Define.AilmentBehaviourType, Element>
            {
                DictionaryList() : base(x => x.type) { }
            }
        }
    }
}
