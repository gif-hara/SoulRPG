using System;
using System.Collections.Generic;
using HK;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [CreateAssetMenu(menuName = "SoulRPG/GameRule")]
    public sealed class GameRule : ScriptableObject
    {
        [SerializeField]
        private CharacterGrowthParameter playerGrowthParameter;
        public CharacterGrowthParameter PlayerGrowthParameter => playerGrowthParameter;

        [SerializeField]
        private int hitPointAmountRate;
        public int HitPointAmountRate => hitPointAmountRate;

        [SerializeField]
        private int staminaAmountRate;
        public int StaminaAmountRate => staminaAmountRate;

        [SerializeField]
        private int hitPointMax;
        public int HitPointMax => hitPointMax;

        [SerializeField]
        private int staminaMax;
        public int StaminaMax => staminaMax;

        [SerializeField]
        private int hitPointGaugeAmount;
        public int HitPointGaugeAmount => hitPointGaugeAmount;

        [SerializeField]
        private int staminaGaugeAmount;
        public int StaminaGaugeAmount => staminaGaugeAmount;

        [SerializeField]
        private ExperienceTableData experienceTableData;
        public ExperienceTableData ExperienceTable => experienceTableData;

        [SerializeField]
        private List<InitialItemData> initialItemDatabase;
        public List<InitialItemData> InitialItemDatabase => initialItemDatabase;

        [SerializeField]
        private EquipmentBlueprint initialEquipment;
        public EquipmentBlueprint InitialEquipment => initialEquipment;

        [SerializeField]
        private AudioData.DictionaryList audioDatabase;
        public AudioData.DictionaryList AudioDatabase => audioDatabase;

        [SerializeField]
        private SequenceData.DictionaryList sequenceDatabase;
        public SequenceData.DictionaryList SequenceDatabase => sequenceDatabase;

        [SerializeField]
        private BattleCharacterSequences playerBattleCharacterSequences;
        public BattleCharacterSequences PlayerBattleCharacterSequences => playerBattleCharacterSequences;

        [SerializeField]
        private int behaviourPointMax;
        public int BehaviourPointMax => behaviourPointMax;

        [SerializeField]
        private int magicCountMax;
        public int MagicCountMax => magicCountMax;

        [SerializeField]
        private int knifeCountMax;
        public int KnifeCountMax => knifeCountMax;

        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float pinchThreshold;
        public float PinchThreshold => pinchThreshold;

        [SerializeField]
        private int debugAddExperience;
        public int DebugAddExperience => debugAddExperience;

        [SerializeField]
        private string defaultSealedName;
        public string DefaultSealedName => defaultSealedName;

        [SerializeField]
        private ScriptableSequences sealedNameSubmitSequences;
        public List<ISequence> SealedNameSubmitSequences => sealedNameSubmitSequences.Sequences;

        [SerializeField]
        private string gameTitle;
        public string GameTitle => gameTitle;

        [SerializeField]
        private List<string> defaultPlayerNames;
        public List<string> DefaultPlayerNames => defaultPlayerNames;

        [Serializable]
        public class ExperienceTableData
        {
            [SerializeField]
            private float rate;

            public int GetNeedExperience(int level)
            {
                return Mathf.FloorToInt(Mathf.Pow(level, 2) * rate);
            }
        }

        [Serializable]
        public class InitialItemData
        {
            [SerializeField]
            private int itemId;
            public int ItemId => itemId;

            [SerializeField]
            private int count;
            public int Count => count;
        }

        [Serializable]
        public class AudioData
        {
            [SerializeField]
            private AudioClip clip;
            public AudioClip Clip => clip;

            [Serializable]
            public class DictionaryList : DictionaryList<string, AudioData>
            {
                public DictionaryList() : base(x => x.clip.name) { }
            }
        }

        [Serializable]
        public class SequenceData
        {
            [SerializeField]
            private ScriptableSequences sequences;
            public List<ISequence> Sequences => sequences.Sequences;

            [Serializable]
            public class DictionaryList : DictionaryList<string, SequenceData>
            {
                public DictionaryList() : base(x => x.sequences.name) { }
            }
        }
    }
}
