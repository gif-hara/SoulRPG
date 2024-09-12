using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using SoulRPG.CharacterControllers;
using UnityEngine;
using UnitySequencerSystem;
using UnitySequencerSystem.Resolvers;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class BeginBattle : ISequence
    {
        [SerializeReference, SubclassSelector]
        private IntResolver enemyIdResolver;

        public async UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var result = await TinyServiceLocator.Resolve<DungeonController>().BeginBattleAsync(
                TinyServiceLocator.Resolve<Character>("Player"),
                enemyIdResolver.Resolve(container)
            );
            container.RegisterOrReplace("BattleResult", result == Define.BattleResult.PlayerWin ? 0 : 1);
        }
    }
}
