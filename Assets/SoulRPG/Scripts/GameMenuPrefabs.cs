using System;
using HK;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class GameMenuPrefabs
    {
        [SerializeField]
        private HKUIDocument rootPrefab;
        public HKUIDocument RootPrefab => rootPrefab;
    }
}
