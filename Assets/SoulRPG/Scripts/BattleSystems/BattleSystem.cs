using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;

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
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            gameEvents.OnBeginBattle.OnNext(this);
            await gameEvents.ShowMessageAndWaitForSubmitInputAsync(new($"{Enemy.BattleStatus.NameWithTag}が現れた。", "Sfx.EnemyAppearance.0"));
            var firstActor = Player.BattleStatus.Speed > Enemy.BattleStatus.Speed ? Player : Enemy;
            var secondActor = firstActor == Player ? Enemy : Player;

            while (!IsBattleEnd())
            {
                await ProcessActorAction(firstActor, secondActor);
                await ProcessActorAction(secondActor, firstActor);
            }

            var result = Player.BattleStatus.IsDead ? Define.BattleResult.PlayerLose : Define.BattleResult.PlayerWin;
            var deadActor = result == Define.BattleResult.PlayerWin ? Enemy : Player;
            await deadActor.OnDeadMessageAsync();
            Player.Dispose();
            Enemy.Dispose();
            cts.Cancel();
            cts.Dispose();
            return result;

            async UniTask ProcessActorAction(BattleCharacter actor, BattleCharacter target)
            {
                await actor.TurnStartAsync(target);
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
