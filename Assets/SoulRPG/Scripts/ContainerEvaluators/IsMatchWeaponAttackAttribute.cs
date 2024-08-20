using System;
using System.Collections.Generic;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG.ContainerEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class IsMatchWeaponAttackAttribute : IContainerEvaluator
    {
        [SerializeField]
        private List<Define.AttackAttribute> attackAttributes;

        public bool Evaluate(Container container)
        {
            var weaponAttackAttribute = container.Resolve<Define.AttackAttribute>("WeaponAttackAttribute");
            return attackAttributes.Contains(weaponAttackAttribute);
        }
    }
}
