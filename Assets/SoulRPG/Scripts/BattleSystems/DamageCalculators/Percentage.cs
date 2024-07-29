using System;
using UnityEngine;

namespace SoulRPG.BattleSystems.DamageCalculators
{
    /// <summary>
    /// 割合ダメージ計算
    /// </summary>
    [Serializable]
    public sealed class Percentage : IDamageCalculator
    {
        [SerializeField]
        private float rate = 1.0f;
        
        public int Calculate(BattleCharacter attacker, BattleCharacter defender, MasterData.Weapon weapon)
        {
            return Mathf.FloorToInt(defender.BattleStatus.HitPointMax * rate);
        }
    }
}
