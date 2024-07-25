using System;
using Unity.VisualScripting;
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

        public void AddVitality(int value)
        {
            vitality += value;
        }

        public void AddStamina(int value)
        {
            stamina += value;
        }

        public void AddPhysicalStrength(int value)
        {
            physicalStrength += value;
        }

        public void AddMagicalStrength(int value)
        {
            magicalStrength += value;
        }

        public void AddSpeed(int value)
        {
            speed += value;
        }
    }
}
