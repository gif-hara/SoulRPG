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
                var canBehaviour = await actor.TurnStartAsync(target);
                while (canBehaviour && !IsBattleEnd() && actor.BattleStatus.CanBehaviour())
                {
                    var commandInvoker = await actor.ThinkAsync(target);
                    if (commandInvoker == null)
                    {
                        break;
                    }
                    var needBehaviourPoint = await actor.GetFixedNeedBehaviourPointAsync(commandInvoker.GetNeedBehaviourPoint());
                    var needStamina = commandInvoker.GetNeedStamina();
#if DEBUG
                    if (TinyServiceLocator.Resolve<BattleDebugData>().NoCost)
                    {
                        needBehaviourPoint = 0;
                        needStamina = 0;
                    }
#endif
                    actor.BattleStatus.AddBehaviourPoint(-needBehaviourPoint);
                    actor.BattleStatus.AddStamina(-needStamina);
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
