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

            while (!IsBattleEnd())
            {
                var firstActor = player.BattleStatus.Speed > enemy.BattleStatus.Speed ? player : enemy;
                var secondActor = firstActor == player ? enemy : player;
                await ProcessActorAction(firstActor, secondActor);
                await firstActor.TurnEndAsync();
                await ProcessActorAction(secondActor, firstActor);
                await secondActor.TurnEndAsync();
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

            async UniTask ProcessActorAction(BattleCharacter actor, BattleCharacter target)
            {
                actor.BattleStatus.RecoveryBehaviourPoint();
                while (!IsBattleEnd() && actor.BattleStatus.CanBehaviour())
                {
                    var commandInvoker = await actor.ThinkAsync();
                    if (commandInvoker == null)
                    {
                        break;
                    }
                    actor.BattleStatus.AddBehaviourPoint(-commandInvoker.GetCost());
                    await commandInvoker.InvokeAsync(actor, target, scope);
                }
            }

            bool IsBattleEnd()
            {
                return player.BattleStatus.IsDead || enemy.BattleStatus.IsDead;
            }
        }
    }
}
