using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.BattleSystems.CommandInvokers;
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
            var cts = CancellationTokenSource.CreateLinkedTokenSource(scope);
            scope = cts.Token;
            var inputController = TinyServiceLocator.Resolve<InputController>();
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            inputController.ChangeInputType(InputController.InputType.UI);

            inputController.InputActions.UI.Submit.OnPerformedAsObservable()
                .Subscribe(_ => gameEvents.OnSubmitInput.OnNext(Unit.Default))
                .RegisterTo(scope);

            gameEvents.OnRequestShowMessage.OnNext($"{enemy.BattleStatus.Name}が現れた");
            await gameEvents.WaitForSubmitInputAsync();

            while (!player.BattleStatus.IsDead && !enemy.BattleStatus.IsDead)
            {
                var playerCommandInvoker = await player.ThinkAsync();
                var enemyCommandInvoker = await enemy.ThinkAsync();
                var playerSpeed = player.BattleStatus.Speed + playerCommandInvoker.GetSpeed();
                var enemySpeed = enemy.BattleStatus.Speed + enemyCommandInvoker.GetSpeed();
                var firstActorData = playerSpeed > enemySpeed ? (player, playerCommandInvoker) : (enemy, enemyCommandInvoker);
                var secondActor = firstActorData.Item1 == player ? (enemy, enemyCommandInvoker) : (player, playerCommandInvoker);
                if (await InvokeSkillActionAsync(firstActorData.Item1, secondActor.Item1, firstActorData.Item2, scope))
                {
                    break;
                }
                if (await InvokeSkillActionAsync(secondActor.Item1, firstActorData.Item1, secondActor.Item2, scope))
                {
                    break;
                }
            }

            var result = player.BattleStatus.IsDead ? Define.BattleResult.PlayerLose : Define.BattleResult.PlayerWin;
            if (result == Define.BattleResult.PlayerWin)
            {
                gameEvents.OnRequestShowMessage.OnNext($"{enemy.BattleStatus.Name}を倒した");
                await gameEvents.WaitForSubmitInputAsync();
            }
            else
            {
                gameEvents.OnRequestShowMessage.OnNext($"{player.BattleStatus.Name}は倒れてしまった");
                await gameEvents.WaitForSubmitInputAsync();
            }
            inputController.ChangeInputType(InputController.InputType.InGame);
            player.Dispose();
            enemy.Dispose();
            cts.Cancel();
            cts.Dispose();
            return result;

            static async UniTask<bool> InvokeSkillActionAsync(BattleCharacter actor, BattleCharacter target, ICommandInvoker commandInvoker, CancellationToken scope)
            {
                await commandInvoker.InvokeAsync(actor, target, scope);
                return actor.BattleStatus.IsDead || target.BattleStatus.IsDead;
            }
        }
    }
}
