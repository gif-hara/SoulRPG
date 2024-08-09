using System.Collections.Generic;
using HK;
using R3;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UserData
    {
        private readonly ReactiveProperty<int> experience = new(0);
        public ReadOnlyReactiveProperty<int> Experience => experience;
        
        public void AddExperience(int value)
        {
            experience.Value += value;
        }
    }
}
