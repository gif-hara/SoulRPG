using System;
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
        public int Level => level;

        [SerializeField]
        private int vitality;
        public int Vitality => vitality;

        [SerializeField]
        public int stamina;
        public int Stamina => stamina;

        [SerializeField]
        private int physicalStrength;
        public int PhysicalStrength => physicalStrength;

        [SerializeField]
        private int magicalStrength;
        public int MagicalStrength => magicalStrength;

        [SerializeField]
        private int speed;
        public int Speed => speed;

        public int HitPoint => (vitality + level) * 8;

        public void AddVitality(int value)
        {
            vitality += value;
            level += value;
        }

        public void AddStamina(int value)
        {
            stamina += value;
            level += value;
        }

        public void AddPhysicalStrength(int value)
        {
            physicalStrength += value;
            level += value;
        }

        public void AddMagicalStrength(int value)
        {
            magicalStrength += value;
            level += value;
        }

        public void AddSpeed(int value)
        {
            speed += value;
            level += value;
        }
    }
}
