using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using SoulRPG.CharacterControllers;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class OpenSavePointView : ISequence
    {
        [SerializeField]
        private HKUIDocument gameMenuBundlePrefab;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var savePointView = new GameSavePointMenuView(gameMenuBundlePrefab, TinyServiceLocator.Resolve<Character>("Player"));
            return savePointView.OpenAsync();
        }
    }
}
