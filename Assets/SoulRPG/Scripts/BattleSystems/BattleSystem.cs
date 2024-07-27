using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
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
                var playerThinkResult = await player.ThinkAsync();
                var enemyThinkResult = await enemy.ThinkAsync();
                await InvokeSkillActionAsync(player, enemy, playerThinkResult.weaponItemId, playerThinkResult.skillId, scope);
                await InvokeSkillActionAsync(enemy, player, enemyThinkResult.weaponItemId, enemyThinkResult.skillId, scope);
            }

            var result = player.BattleStatus.IsDead ? Define.BattleResult.PlayerLose : Define.BattleResult.PlayerWin;
            inputController.ChangeInputType(InputController.InputType.InGame);
            Debug.Log("BattleSystem End");
            return result;

            UniTask InvokeSkillActionAsync(BattleCharacter actor, BattleCharacter target, int weaponId, int skillId, CancellationToken scope)
            {
                var masterDataSkill = skillId.GetMasterDataSkill();
                var sequences = masterDataSkill.ActionSequences.Sequences;
                var container = new Container();
                container.Register("Actor", actor);
                container.Register("Target", target);
                container.Register(weaponId.GetMasterDataWeapon());
                var sequencer = new Sequencer(container, sequences);
                return sequencer.PlayAsync(scope);
            }
        }
    }
}
