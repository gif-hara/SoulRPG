using System;
using System.Collections.Generic;
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

        private readonly HKUIDocument listDocumentPrefab;

        private UniTaskCompletionSource<ICommandInvoker> source;

        private int selectedWeaponId;

        public Input(HKUIDocument commandDocumentPrefab, HKUIDocument listDocumentPrefab)
        {
            this.listDocumentPrefab = listDocumentPrefab;
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

        private UniTask StateNothingAsync(CancellationToken scope)
        {
            return UniTask.CompletedTask;
        }

        private async UniTask StateSelectMainCommandAsync(CancellationToken scope)
        {
            var commands = new List<string>
            {
                "武器",
                "道具",
                "逃亡"
            };
#if DEBUG
            if (TinyServiceLocator.Resolve<BattleDebugData>().IsAllSkillAvailable)
            {
                commands.Add("全スキル");
            }
#endif

            var index = await commandView.CreateCommandsAsync("選べ", commands, 0);
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            switch (index)
            {
                case 0:
                    gameEvents.OnRequestPlaySfx.OnNext("Sfx.Message.0");
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
#if DEBUG
                case 3:
                    gameEvents.OnRequestPlaySfx.OnNext("Sfx.Message.0");
                    stateMachine.Change(StateSelectAllSkillAsync);
                    break;
#endif
            }
        }

        private async UniTask StateSelectWeaponAsync(CancellationToken scope)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            TinyServiceLocator.Resolve<InputController>().InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    gameEvents.OnRequestPlaySfx.OnNext("Sfx.Cancel.0");
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
            selectedWeaponId = index;
            gameEvents.OnRequestPlaySfx.OnNext("Sfx.Message.0");
            stateMachine.Change(StateSelectSkillAsync);
        }

        private async UniTask StateSelectSkillAsync(CancellationToken scope)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            TinyServiceLocator.Resolve<InputController>().InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    gameEvents.OnRequestPlaySfx.OnNext("Sfx.Cancel.0");
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
                    gameEvents.OnRequestShowMessage.OnNext(new("このターンではもう使用出来ない。", "Sfx.Message.0"));
                    continue;
                }
                var behaviourPoint = await character.GetFixedNeedBehaviourPointAsync(skill.NeedBehaviourPoint);
                if (!character.BattleStatus.HasBehaviourPoint(behaviourPoint))
                {
                    gameEvents.OnRequestShowMessage.OnNext(new("BPが足りない。", "Sfx.Message.0"));
                    continue;
                }
                if (!character.BattleStatus.HasStamina(skill.NeedStamina))
                {
                    gameEvents.OnRequestShowMessage.OnNext(new("スタミナが足りない。", "Sfx.Message.0"));
                    continue;
                }
                commandView.Close();
                source.TrySetResult(new Skill(weapon.ItemId, skill.Id, skill.CanRegisterUsedSkills));
                break;
            }
        }

        private async UniTask StateSelectAllSkillAsync(CancellationToken scope)
        {
            commandView.SetActive(false);
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            TinyServiceLocator.Resolve<InputController>().InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    commandView.SetActive(true);
                    gameEvents.OnRequestPlaySfx.OnNext("Sfx.Cancel.0");
                    stateMachine.Change(StateSelectMainCommandAsync);
                })
                .RegisterTo(scope);
            var listElements = TinyServiceLocator.Resolve<MasterData>()
                .Skills
                .List
                .Select(x =>
                {
                    return new Action<HKUIDocument>(element =>
                    {
                        GameListView.ApplyAsSimpleElement(
                            element,
                            $"{x.Id}: {x.Name}",
                            _ =>
                            {
                                source.TrySetResult(new Skill(Define.TestWeaponId, x.Id, x.CanRegisterUsedSkills));
                                commandView.Close();
                                stateMachine.Change(StateNothingAsync);
                            });
                    });
                });
            var listDocument = GameListView.Create(listDocumentPrefab, listElements, 0);
            await scope.WaitUntilCanceled();
            UnityEngine.Object.Destroy(listDocument.gameObject);
        }
    }
}
