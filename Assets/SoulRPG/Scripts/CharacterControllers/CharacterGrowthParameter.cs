using System;
using HK;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class CharacterGrowthParameter
    {
        [SerializeField]
        private int level;
        public int Level { get => level; set => level = value; }

        [SerializeField]
        private int vitality;
        public int Vitality { get => vitality; set => vitality = value; }

        [SerializeField]
        public int stamina;
        public int Stamina { get => stamina; set => stamina = value; }

        [SerializeField]
        private int physicalStrength;
        public int PhysicalStrength { get => physicalStrength; set => physicalStrength = value; }

        [SerializeField]
        private int magicalStrength;
        public int MagicalStrength { get => magicalStrength; set => magicalStrength = value; }

        [SerializeField]
        private int speed;
        public int Speed { get => speed; set => speed = value; }

        public int HitPointMax => (vitality + level) * TinyServiceLocator.Resolve<GameRule>().HitPointAmountRate;

        public int StaminaMax => (stamina + level) * TinyServiceLocator.Resolve<GameRule>().StaminaAmountRate;

        public CharacterGrowthParameter()
        {
        }

        public CharacterGrowthParameter(CharacterGrowthParameter parameter)
        {
            level = parameter.level;
            vitality = parameter.vitality;
            stamina = parameter.stamina;
            physicalStrength = parameter.physicalStrength;
            magicalStrength = parameter.magicalStrength;
            speed = parameter.speed;
        }

        public void Sync(CharacterGrowthParameter parameter)
        {
            level = parameter.level;
            vitality = parameter.vitality;
            stamina = parameter.stamina;
            physicalStrength = parameter.physicalStrength;
            magicalStrength = parameter.magicalStrength;
            speed = parameter.speed;
        }
    }
}
