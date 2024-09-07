using System;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class SaveData
    {
        public PlayerData playerData;

        [Serializable]
        public class PlayerData
        {
            public string name;

            public CharacterGrowthParameter growthParameter;
        }
    }
}
