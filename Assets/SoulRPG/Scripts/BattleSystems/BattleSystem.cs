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
        public static async UniTask<Define.BattleResult> BeginAsync(
            BattleCharacter player,
            BattleCharacter enemy,
            CancellationToken scope
            )
        {
            Debug.Log("BattleSystem Begin");
            var inputController = TinyServiceLocator.Resolve<InputController>();
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            inputController.ChangeInputType(InputController.InputType.UI);

            while (!player.BattleStatus.IsDead && !enemy.BattleStatus.IsDead)
            {
                var playerThinkResult = await player.ThinkAsync();
                var enemyThinkResult = await enemy.ThinkAsync();
                gameEvents.RequestShowMessage.OnNext($"Player: {playerThinkResult.weaponItemId}, {playerThinkResult.skillId}");
                await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: scope);
                gameEvents.RequestShowMessage.OnNext($"Enemy: {enemyThinkResult.weaponItemId}, {enemyThinkResult.skillId}");
                await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: scope);
            }

            var result = player.BattleStatus.IsDead ? Define.BattleResult.PlayerLose : Define.BattleResult.PlayerWin;
            inputController.ChangeInputType(InputController.InputType.InGame);
            Debug.Log("BattleSystem End");
            return result;
        }
    }
}
