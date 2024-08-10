using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [CreateAssetMenu(menuName = "SoulRPG/GameRule")]
    public sealed class GameRule : ScriptableObject
    {
        [SerializeField]
        private ExperienceTableData experienceTableData;
        public ExperienceTableData ExperienceTable => experienceTableData;

        [SerializeField]
        private List<InitialItemData> initialItemDatabase;
        public List<InitialItemData> InitialItemDatabase => initialItemDatabase;

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
    }
}
