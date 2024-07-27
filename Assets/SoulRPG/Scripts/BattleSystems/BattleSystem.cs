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
            inputController.ChangeInputType(InputController.InputType.UI);

            while (!player.BattleStatus.IsDead && !enemy.BattleStatus.IsDead)
            {
                player.BattleStatus.TakeDamage(enemy.BattleStatus.PhysicalAttack.CurrentValue);
                Debug.Log($"Player HP: {player.BattleStatus.HitPoint.CurrentValue}");
                await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: scope);
                enemy.BattleStatus.TakeDamage(player.BattleStatus.PhysicalAttack.CurrentValue);
                Debug.Log($"Enemy HP: {enemy.BattleStatus.HitPoint.CurrentValue}");
                await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: scope);
            }

            var result = player.BattleStatus.IsDead ? Define.BattleResult.PlayerLose : Define.BattleResult.PlayerWin;
            inputController.ChangeInputType(InputController.InputType.InGame);
            Debug.Log("BattleSystem End");
            return result;
        }
    }
}
