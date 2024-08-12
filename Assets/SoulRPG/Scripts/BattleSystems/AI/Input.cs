using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.BattleSystems.CommandInvokers;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class Input : IBattleAI
    {
        private BattleCharacter character;

        private readonly TinyStateMachine stateMachine = new();

        private readonly CommandView commandView;

        private UniTaskCompletionSource<ICommandInvoker> source;

        private int selectedWeaponId;

        public Input(HKUIDocument commandDocumentPrefab)
        {
            commandView = new CommandView(commandDocumentPrefab);
        }

        public void Dispose()
        {
            stateMachine.Dispose();
        }

        public UniTask<ICommandInvoker> ThinkAsync(BattleCharacter character)
        {
            this.character = character;
            commandView.Open();
            stateMachine.Change(StateSelectMainCommandAsync);
            source = new UniTaskCompletionSource<ICommandInvoker>();
            return source.Task;
        }

        private async UniTask StateSelectMainCommandAsync(CancellationToken scope)
        {
            var commands = new[]
            {
                "武器",
                "道具",
                "逃亡"
            };

            var index = await commandView.CreateCommandsAsync("選べ", commands, 0);
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            switch (index)
            {
                case 0:
                    stateMachine.Change(StateSelectWeaponAsync);
                    break;
                case 1:
                    gameEvents.OnRequestShowMessage.OnNext(new("どうやら未実装のようだ", "Sfx.Message.0"));
                    stateMachine.Change(StateSelectMainCommandAsync);
                    break;
                case 2:
                    gameEvents.OnRequestShowMessage.OnNext(new("どうやら未実装のようだ", "Sfx.Message.0"));
                    stateMachine.Change(StateSelectMainCommandAsync);
                    break;
            }
        }

        private async UniTask StateSelectWeaponAsync(CancellationToken scope)
        {
            TinyServiceLocator.Resolve<InputController>().InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(StateSelectMainCommandAsync);
                })
                .RegisterTo(scope);
            var commands = character.Equipment.GetWeaponIds()
                .Select(x =>
                {
                    if (x.TryGetMasterDataWeapon(out var weapon))
                    {
                        return weapon.ItemId.GetMasterDataItem().Name;
                    }
                    return Define.HandWeaponId.GetMasterDataItem().Name;
                });
            var index = await commandView.CreateCommandsAsync("武器を選べ", commands, 0);
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            selectedWeaponId = index;
            stateMachine.Change(StateSelectSkillAsync);
        }

        private async UniTask StateSelectSkillAsync(CancellationToken scope)
        {
            TinyServiceLocator.Resolve<InputController>().InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(StateSelectWeaponAsync);
                })
                .RegisterTo(scope);
            MasterData.Weapon weapon;
            if (!character.Equipment.GetWeaponId(selectedWeaponId).TryGetMasterDataWeapon(out weapon))
            {
                weapon = Define.HandWeaponId.GetMasterDataWeapon();
            }
            var skills = weapon.Skills
                .Where(x => x.ContainsMasterDataSkill())
                .Select(x => x.GetMasterDataSkill())
                .ToList();
            var commands = skills.Select(x => x.Name);
            var selectedIndex = 0;
            while (true)
            {
                var index = await commandView.CreateCommandsAsync("スキルを選べ", commands, selectedIndex);
                var skill = skills.ElementAt(index);
                selectedIndex = index;
                var identifier = Skill.CreateIdentifier(weapon.ItemId, skill.Id);
                if (character.UsedSkills.Contains(identifier))
                {
                    TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext(new("このターンではもう使用出来ない。", "Sfx.Message.0"));
                    continue;
                }
                var behaviourPoint = await character.GetFixedNeedBehaviourPointAsync(skill.NeedBehaviourPoint);
                if (!character.BattleStatus.HasBehaviourPoint(behaviourPoint))
                {
                    TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext(new("BPが足りない。", "Sfx.Message.0"));
                    continue;
                }
                if (!character.BattleStatus.HasStamina(skill.NeedStamina))
                {
                    TinyServiceLocator.Resolve<GameEvents>().OnRequestShowMessage.OnNext(new("スタミナが足りない。", "Sfx.Message.0"));
                    continue;
                }
                commandView.Close();
                source.TrySetResult(new Skill(weapon.ItemId, skill.Id, skill.CanRegisterUsedSkills));
                break;
            }
        }
    }
}
