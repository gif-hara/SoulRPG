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
    public sealed class IsMatchAttackAttribute : IContainerEvaluator
    {
        [SerializeField]
        private List<Define.AttackAttribute> attackAttributes;

        public bool Evaluate(Container container)
        {
            var damageData = container.Resolve<DamageData>("DamageData");
            return attackAttributes.Contains(damageData.AttackAttribute);
        }
    }
}
