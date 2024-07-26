using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
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
            Debug.Log("BattleSystem Begin");
            var inputController = TinyServiceLocator.Resolve<InputController>();
            inputController.ChangeInputType(InputController.InputType.UI);
            await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: scope);
            inputController.ChangeInputType(InputController.InputType.InGame);
            Debug.Log("BattleSystem End");
            return Define.BattleResult.PlayerWin;
        }
    }
}
