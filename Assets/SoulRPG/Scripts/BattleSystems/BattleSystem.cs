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
                if (await InvokeSkillActionAsync(actorData.firstActor.actor, actorData.secondActor.actor, actorData.firstActor.commandInvoker, scope))
                {
                    break;
                }
                if (await InvokeSkillActionAsync(actorData.secondActor.actor, actorData.firstActor.actor, actorData.secondActor.commandInvoker, scope))
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
            static ((BattleCharacter actor, ICommandInvoker commandInvoker) firstActor, (BattleCharacter actor, ICommandInvoker commandInvoker) secondActor) GetActorData(
                BattleCharacter player,
                ICommandInvoker playerCommandInvoker,
                BattleCharacter enemy,
                ICommandInvoker enemyCommandInvoker
                )
            {
                var playerData = (player, playerCommandInvoker);
                var enemyData = (enemy, enemyCommandInvoker);
                var playerSpeed = player.BattleStatus.Speed + playerCommandInvoker.GetSpeed();
                var enemySpeed = enemy.BattleStatus.Speed + enemyCommandInvoker.GetSpeed();
                return playerSpeed > enemySpeed ? (playerData, enemyData) : (enemyData, playerData);
            }
        }
    }
}
