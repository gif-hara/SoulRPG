using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class EquipmentBlueprint
    {
        [SerializeField]
        private List<int> weaponIds = new();
        public List<int> WeaponIds => weaponIds;

        [SerializeField]
        private int headId;
        public int HeadId => headId;

        [SerializeField]
        private int bodyId;
        public int BodyId => bodyId;

        [SerializeField]
        private int armId;
        public int ArmId => armId;

        [SerializeField]
        private int legId;
        public int LegId => legId;

        [SerializeField]
        private List<int> accessoryIds = new();
        public List<int> AccessoryIds => accessoryIds;
    }
}
