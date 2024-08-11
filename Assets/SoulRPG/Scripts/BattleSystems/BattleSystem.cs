using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using UnityEngine;

namespace SoulRPG.BattleSystems
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class BattleSystem
    {
        private readonly BattleCharacter player;

        private readonly BattleCharacter enemy;

        public BattleSystem(BattleCharacter player, BattleCharacter enemy)
        {
            this.player = player;
            this.enemy = enemy;
        }
        public async UniTask<Define.BattleResult> BeginAsync(CancellationToken scope)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(scope);
            scope = cts.Token;
            var inputController = TinyServiceLocator.Resolve<InputController>();
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            gameEvents.OnBeginBattle.OnNext(this);
            inputController.ChangeInputType(InputController.InputType.UI);

            inputController.InputActions.UI.Submit.OnPerformedAsObservable()
                .Subscribe(_ => gameEvents.OnSubmitInput.OnNext(Unit.Default))
                .RegisterTo(scope);

            await gameEvents.ShowMessageAndWaitForSubmitInputAsync(new($"{enemy.BattleStatus.NameWithTag}が現れた。", "Sfx.EnemyAppearance.0"));
            var firstActor = player.BattleStatus.Speed > enemy.BattleStatus.Speed ? player : enemy;
            var secondActor = firstActor == player ? enemy : player;

            while (!IsBattleEnd())
            {
                await ProcessActorAction(firstActor, secondActor);
                await ProcessActorAction(secondActor, firstActor);
            }

            var result = player.BattleStatus.IsDead ? Define.BattleResult.PlayerLose : Define.BattleResult.PlayerWin;
            if (result == Define.BattleResult.PlayerWin)
            {
                await gameEvents.ShowMessageAndWaitForSubmitInputAsync(new($"{enemy.BattleStatus.NameWithTag}を倒した。", "Sfx.Defeat.0"));
            }
            else
            {
                await gameEvents.ShowMessageAndWaitForSubmitInputAsync(new($"{player.BattleStatus.NameWithTag}は倒れてしまった。", "Sfx.Defeat.0"));
            }
            inputController.ChangeInputType(InputController.InputType.InGame);
            player.Dispose();
            enemy.Dispose();
            cts.Cancel();
            cts.Dispose();
            return result;

            async UniTask ProcessActorAction(BattleCharacter actor, BattleCharacter target)
            {
                await actor.TurnStartAsync();
                while (!IsBattleEnd() && actor.BattleStatus.CanBehaviour())
                {
                    var commandInvoker = await actor.ThinkAsync();
                    if (commandInvoker == null)
                    {
                        break;
                    }
                    var cost = await actor.GetFixedNeedBehaviourPointAsync(commandInvoker.GetCost());
                    actor.BattleStatus.AddBehaviourPoint(-cost);
                    await commandInvoker.InvokeAsync(actor, target, scope);
                    await actor.OnBehaviourEndAsync(target, scope);
                }
                await actor.InvokeAfterCommandAsync(target, scope);
                await actor.TurnEndAsync();
            }

            bool IsBattleEnd()
            {
                return player.BattleStatus.IsDead || enemy.BattleStatus.IsDead;
            }
        }
    }
}
