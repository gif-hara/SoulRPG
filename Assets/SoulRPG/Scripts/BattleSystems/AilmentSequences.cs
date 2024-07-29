using System;
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
        private ScriptableSequences onAdded;
        public ScriptableSequences OnAdded => onAdded;

        [SerializeField]
        private ScriptableSequences onTurnEnd;
        public ScriptableSequences OnTurnEnd => onTurnEnd;

        [SerializeField]
        private ScriptableSequences onRemoved;
        public ScriptableSequences OnRemoved => onRemoved;

        [SerializeField]
        private ScriptableSequences canExecutableTurn;
        public ScriptableSequences CanExecutableTurn => canExecutableTurn;

        [SerializeField]
        private ScriptableSequences onComboFromGivedDamage;
        public ScriptableSequences OnComboFromGivedDamage => onComboFromGivedDamage;

        [SerializeField]
        private ScriptableSequences onComboFromTakedDamage;
        public ScriptableSequences OnComboFromTakedDamage => onComboFromTakedDamage;
    }
}
