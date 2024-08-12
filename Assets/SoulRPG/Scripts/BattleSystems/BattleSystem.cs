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
        public BattleCharacter Player { get; }

        public BattleCharacter Enemy { get; }

        public BattleSystem(BattleCharacter player, BattleCharacter enemy)
        {
            Player = player;
            Enemy = enemy;
        }
        public async UniTask<Define.BattleResult> BeginAsync(CancellationToken scope)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(scope);
            scope = cts.Token;
            var inputController = TinyServiceLocator.Resolve<InputController>();
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            gameEvents.OnBeginBattle.OnNext(this);
            inputController.ChangeInputType(InputController.InputType.UI);
            await gameEvents.ShowMessageAndWaitForSubmitInputAsync(new($"{Enemy.BattleStatus.NameWithTag}が現れた。", "Sfx.EnemyAppearance.0"));
            var firstActor = Player.BattleStatus.Speed > Enemy.BattleStatus.Speed ? Player : Enemy;
            var secondActor = firstActor == Player ? Enemy : Player;

            while (!IsBattleEnd())
            {
                await ProcessActorAction(firstActor, secondActor);
                await ProcessActorAction(secondActor, firstActor);
            }

            var result = Player.BattleStatus.IsDead ? Define.BattleResult.PlayerLose : Define.BattleResult.PlayerWin;
            if (result == Define.BattleResult.PlayerWin)
            {
                Enemy.Events.OnDeadMessage.OnNext(Unit.Default);
                await gameEvents.ShowMessageAndWaitForSubmitInputAsync(new($"{Enemy.BattleStatus.NameWithTag}を倒した。", "Sfx.Defeat.0"));
            }
            else
            {
                Player.Events.OnDeadMessage.OnNext(Unit.Default);
                await gameEvents.ShowMessageAndWaitForSubmitInputAsync(new($"{Player.BattleStatus.NameWithTag}は倒れてしまった。", "Sfx.Defeat.0"));
            }
            inputController.ChangeInputType(InputController.InputType.InGame);
            Player.Dispose();
            Enemy.Dispose();
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
                    var needBehaviourPoint = await actor.GetFixedNeedBehaviourPointAsync(commandInvoker.GetNeedBehaviourPoint());
                    actor.BattleStatus.AddBehaviourPoint(-needBehaviourPoint);
                    actor.BattleStatus.AddStamina(-commandInvoker.GetNeedStamina());
                    await commandInvoker.InvokeAsync(actor, target, scope);
                    await actor.OnBehaviourEndAsync(target, scope);
                }
                await actor.InvokeAfterCommandAsync(target, scope);
                await actor.TurnEndAsync();
            }

            bool IsBattleEnd()
            {
                return Player.BattleStatus.IsDead || Enemy.BattleStatus.IsDead;
            }
        }
    }
}
