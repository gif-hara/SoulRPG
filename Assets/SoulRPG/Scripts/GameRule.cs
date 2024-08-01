using System;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class GameRule
    {
        [SerializeField]
        private ExperienceTableData experienceTableData;
        public ExperienceTableData ExperienceTable => experienceTableData;

        public class ExperienceTableData
        {
            [SerializeField]
            private int rate;

            public int GetNeedExperience(int level)
            {
                return Mathf.FloorToInt(Mathf.Pow(level, 2) * rate);
            }
        }
    }
}
