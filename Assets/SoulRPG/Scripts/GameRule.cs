using System;
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
    }
}
