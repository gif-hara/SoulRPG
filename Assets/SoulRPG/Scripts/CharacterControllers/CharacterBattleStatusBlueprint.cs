using System;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class CharacterBattleStatusBlueprint
    {
        [SerializeField]
        private string name;
        public string Name => name;

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

        [SerializeField]
        private int experience;
        public int Experience => experience;

        [SerializeField]
        private Define.CharacterAttribute attribute;
        public Define.CharacterAttribute Attribute => attribute;
    }
}
