using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.BattleSystems.CommandInvokers;
using UnityEngine;
using UnitySequencerSystem;

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

            inputController.InputActions.UI.Submit.OnPerformedAsObservable()
                .Subscribe(_ => gameEvents.OnSubmitInput.OnNext(Unit.Default))
                .RegisterTo(scope);

            while (!player.BattleStatus.IsDead && !enemy.BattleStatus.IsDead)
            {
                var playerCommandInvoker = await player.ThinkAsync();
                var enemyCommandInvoker = await enemy.ThinkAsync();
                if (await InvokeSkillActionAsync(player, enemy, playerCommandInvoker, scope))
                {
                    break;
                }
                if (await InvokeSkillActionAsync(enemy, player, enemyCommandInvoker, scope))
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
            Debug.Log("BattleSystem End");
            return result;

            static async UniTask<bool> InvokeSkillActionAsync(BattleCharacter actor, BattleCharacter target, ICommandInvoker commandInvoker, CancellationToken scope)
            {
                await commandInvoker.InvokeAsync(actor, target, scope);
                return actor.BattleStatus.IsDead || target.BattleStatus.IsDead;
            }
        }
    }
}
