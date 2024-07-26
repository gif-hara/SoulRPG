using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SoulRPG.BattleSystems
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class BattleSystem
    {
        public BattleSystem()
        {
        }

        public async UniTask<Define.BattleResult> BeginAsync(CancellationToken scope)
        {
            Debug.Log("BattleSystem BeginAsync");
            await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: scope);
            return Define.BattleResult.PlayerWin;
        }
    }
}
