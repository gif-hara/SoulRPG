using System;
using SoulRPG.CharacterControllers;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class CharacterBattleStatus
    {
        [SerializeField]
        private int hitPoint;
        public int HitPoint => hitPoint;

        [SerializeField]
        private int stamina;
        public int Stamina => stamina;
        
        [SerializeField]
        private int physicalAttack;
        public int PhysicalAttack => physicalAttack;
        
        [SerializeField]
        private int magicalAttack;
        public int MagicalAttack => magicalAttack;
        
        [SerializeField]
        private float slashCutRate;
        public float SlashCutRate => slashCutRate;
        
        [SerializeField]
        private float blowCutRate;
        public float BlowCutRate => blowCutRate;
        
        [SerializeField]
        private float thrustCutRate;
        public float ThrustCutRate => thrustCutRate;
        
        [SerializeField]
        private float magicCutRate;
        public float MagicCutRate => magicCutRate;
        
        [SerializeField]
        private float fireCutRate;
        public float FireCutRate => fireCutRate;
        
        [SerializeField]
        private float thunderCutRate;
        public float ThunderCutRate => thunderCutRate;
        
        [SerializeField]
        private int speed;
        public int Speed => speed;

        public CharacterBattleStatus(Character character)
        {
            var growthParameter = character.GrowthParameter;
            var equipment = character.Equipment;
            hitPoint = growthParameter.Vitality * 8;
            stamina = growthParameter.Stamina;
            physicalAttack = growthParameter.PhysicalStrength;
            magicalAttack = growthParameter.MagicalStrength;
            slashCutRate = equipment.TotalSlashCutRate;
            blowCutRate = equipment.TotalBlowCutRate;
            thrustCutRate = equipment.TotalThrustCutRate;
            magicCutRate = equipment.TotalMagicCutRate;
            fireCutRate = equipment.TotalFireCutRate;
            thunderCutRate = equipment.TotalThunderCutRate;
            speed = growthParameter.Speed;
        }
    }
}
