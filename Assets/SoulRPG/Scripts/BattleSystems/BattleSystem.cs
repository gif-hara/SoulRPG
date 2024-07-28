using System.Threading;
using System.Windows.Input;
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
                var actorData = GetActorData(player, playerCommandInvoker, enemy, enemyCommandInvoker);
                if (await InvokeCommandAsync(player, playerCommandInvoker, enemy, enemyCommandInvoker, scope))
                {
                    break;
                }
                while (
                    !player.BattleStatus.IsDead && !enemy.BattleStatus.IsDead &&
                    (player.AfterCommandInvoker != null || enemy.AfterCommandInvoker != null)
                    )
                {
                    playerCommandInvoker = player.AfterCommandInvoker;
                    enemyCommandInvoker = enemy.AfterCommandInvoker;
                    player.AfterCommandInvoker = null;
                    enemy.AfterCommandInvoker = null;
                    if (await InvokeCommandAsync(player, playerCommandInvoker, enemy, enemyCommandInvoker, scope))
                    {
                        break;
                    }
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

            static ((BattleCharacter actor, ICommandInvoker commandInvoker) firstActor, (BattleCharacter actor, ICommandInvoker commandInvoker) secondActor) GetActorData(
                BattleCharacter player,
                ICommandInvoker playerCommandInvoker,
                BattleCharacter enemy,
                ICommandInvoker enemyCommandInvoker
                )
            {
                var playerData = (player, playerCommandInvoker);
                var enemyData = (enemy, enemyCommandInvoker);
                var playerBehaviourPriority = playerCommandInvoker == null ? -99 : playerCommandInvoker.GetBehaviourPriority();
                var enemyBehaviourPriority = enemyCommandInvoker == null ? -99 : enemyCommandInvoker.GetBehaviourPriority();
                if (playerBehaviourPriority > enemyBehaviourPriority)
                {
                    return (playerData, enemyData);
                }
                if (playerBehaviourPriority < enemyBehaviourPriority)
                {
                    return (enemyData, playerData);
                }
                var playerSpeed = player.BattleStatus.Speed + playerCommandInvoker?.GetSpeed() ?? 0;
                var enemySpeed = enemy.BattleStatus.Speed + enemyCommandInvoker?.GetSpeed() ?? 0;
                return playerSpeed > enemySpeed ? (playerData, enemyData) : (enemyData, playerData);
            }
            static async UniTask<bool> InvokeCommandAsync(
                BattleCharacter player,
                ICommandInvoker playerCommandInvoker,
                BattleCharacter enemy,
                ICommandInvoker enemyCommandInvoker,
                CancellationToken scope
                )
            {
                var (firstActor, secondActor) = GetActorData(player, playerCommandInvoker, enemy, enemyCommandInvoker);
                if (await InvokeAsync(firstActor.actor, secondActor.actor, firstActor.commandInvoker, scope))
                {
                    return true;
                }
                if (await InvokeAsync(secondActor.actor, firstActor.actor, secondActor.commandInvoker, scope))
                {
                    return true;
                }
                return false;
                static async UniTask<bool> InvokeAsync(BattleCharacter actor, BattleCharacter target, ICommandInvoker commandInvoker, CancellationToken scope)
                {
                    if (commandInvoker == null)
                    {
                        return false;
                    }
                    await commandInvoker.InvokeAsync(actor, target, scope);
                    return actor.BattleStatus.IsDead || target.BattleStatus.IsDead;
                }
            }
        }
    }
}
