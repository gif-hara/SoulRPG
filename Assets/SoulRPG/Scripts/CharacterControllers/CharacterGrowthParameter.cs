using System;
using HK;
using SoulRPG.CharacterControllers;
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
        private int stamina;
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

        [SerializeField]
        private int behaviourPoint;
        public int BehaviourPoint { get => behaviourPoint; set => behaviourPoint = value; }

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
            behaviourPoint = parameter.behaviourPoint;
        }

        public void Sync(CharacterGrowthParameter parameter, Character character)
        {
            level = parameter.level;
            vitality = parameter.vitality;
            stamina = parameter.stamina;
            physicalStrength = parameter.physicalStrength;
            magicalStrength = parameter.magicalStrength;
            speed = parameter.speed;
            behaviourPoint = parameter.behaviourPoint;
            character.Events.OnSyncGrowthParameter.OnNext(this);
        }
    }
}
