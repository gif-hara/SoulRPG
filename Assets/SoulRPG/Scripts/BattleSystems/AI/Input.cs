using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.BattleSystems.CommandInvokers;
using TMPro;
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

        private readonly HKUIDocument commandDocumentPrefab;

        private readonly HKUIDocument listDocumentPrefab;

        private readonly HKUIDocument ailmentInformationDocumentPrefab;

        private readonly HKUIDocument informationWeaponDocumentPrefab;

        private UniTaskCompletionSource<ICommandInvoker> source;

        private int selectedWeaponId;

        private CancellationTokenSource informationWeaponScope;

        public Input(
            HKUIDocument commandDocumentPrefab,
            HKUIDocument listDocumentPrefab,
            HKUIDocument ailmentInformationDocumentPrefab,
            HKUIDocument informationWeaponDocumentPrefab
            )
        {
            this.listDocumentPrefab = listDocumentPrefab;
            this.commandDocumentPrefab = commandDocumentPrefab;
            this.ailmentInformationDocumentPrefab = ailmentInformationDocumentPrefab;
            this.informationWeaponDocumentPrefab = informationWeaponDocumentPrefab;
        }

        public void Dispose()
        {
            stateMachine.Dispose();
        }

        public UniTask<ICommandInvoker> ThinkAsync(BattleCharacter character)
        {
            this.character = character;
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
            informationWeaponScope?.Cancel();
            informationWeaponScope?.Dispose();
            informationWeaponScope = null;
            var commands = new List<Action<HKUIDocument>>
            {
                new(x =>
                {
                    GameListView.ApplyAsSimpleElement(
                        x,
                        "武器",
                        _ =>
                        {
                            TinyServiceLocator.Resolve<GameEvents>().OnRequestPlaySfx.OnNext("Sfx.Message.0");
                            stateMachine.Change(StateSelectWeaponAsync);
                        },
                        _ =>
                        {
                            GameTipsView.SetTip("武器を利用して攻撃などを行う。");
                        });
                }),
                new(x =>
                {
                    GameListView.ApplyAsSimpleElement(
                        x,
                        "確認",
                        _ =>
                        {
                            TinyServiceLocator.Resolve<GameEvents>().OnRequestPlaySfx.OnNext("Sfx.Message.0");
                            stateMachine.Change(StateConfirmAsync);
                        },
                        _ =>
                        {
                            GameTipsView.SetTip("状態異常・ステータスなどの情報を確認する。");
                        });
                })
            };
#if DEBUG
            if (TinyServiceLocator.Resolve<BattleDebugData>().IsAllSkillAvailable)
            {
                commands.Add(
                    x =>
                    {
                        GameListView.ApplyAsSimpleElement(
                            x,
                            "全スキル",
                            _ =>
                            {
                                TinyServiceLocator.Resolve<GameEvents>().OnRequestPlaySfx.OnNext("Sfx.Message.0");
                                stateMachine.Change(StateSelectAllSkillAsync);
                            });
                    }
                );
            }
#endif
            var listDocument = GameListView.CreateAsCommand(commandDocumentPrefab, commands, 0);
            listDocument.Q<TMP_Text>("Header").text = "選べ";
            await scope.WaitUntilCanceled();
            if (listDocument != null)
            {
                UnityEngine.Object.Destroy(listDocument.gameObject);
            }
        }

        private async UniTask StateSelectWeaponAsync(CancellationToken scope)
        {
            informationWeaponScope?.Cancel();
            informationWeaponScope?.Dispose();
            informationWeaponScope = new CancellationTokenSource();
            var informationWeaponView = new BattleInformationWeaponView(informationWeaponDocumentPrefab, informationWeaponScope.Token);
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            var commands = character.Equipment.GetWeaponIds()
                .Select((weaponId, index) =>
                {
                    return new Action<HKUIDocument>(element =>
                    {
                        var fixedWeaponId = weaponId.TryGetMasterDataWeapon(out var weapon)
                            ? weapon.ItemId
                            : Define.HandWeaponId;
                        GameListView.ApplyAsSimpleElement(
                            element,
                            fixedWeaponId.GetMasterDataItem().Name,
                            _ =>
                            {
                                selectedWeaponId = index;
                                gameEvents.OnRequestPlaySfx.OnNext("Sfx.Message.0");
                                stateMachine.Change(StateSelectSkillAsync);
                            },
                            _ =>
                            {
                                GameTipsView.SetTip($"{fixedWeaponId.GetMasterDataItem().Name}を使って行動する。");
                                informationWeaponView.Setup(fixedWeaponId);
                            });
                    });
                });
            var listDocument = GameListView.CreateAsCommand(commandDocumentPrefab, commands, 0);
            listDocument.Q<TMP_Text>("Header").text = "武器を選べ";
            TinyServiceLocator.Resolve<InputController>().InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    gameEvents.OnRequestPlaySfx.OnNext("Sfx.Cancel.0");
                    stateMachine.Change(StateSelectMainCommandAsync);
                })
                .RegisterTo(scope);
            await scope.WaitUntilCanceled();
            if (listDocument != null)
            {
                UnityEngine.Object.Destroy(listDocument.gameObject);
            }
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
            var commands = skills.Select(x =>
            {
                return new Action<HKUIDocument>(element =>
                {
                    GameListView.ApplyAsSimpleElement(
                        element,
                        x.Name,
                        async _ =>
                        {
                            gameEvents.OnRequestPlaySfx.OnNext("Sfx.Message.0");
                            var identifier = Skill.CreateIdentifier(weapon.ItemId, x.Id);
                            if (character.UsedSkills.Contains(identifier))
                            {
                                gameEvents.OnRequestShowMessage.OnNext(new("このターンではもう使用出来ない。", "Sfx.Message.0"));
                                return;
                            }
                            var behaviourPoint = await character.GetFixedNeedBehaviourPointAsync(x.NeedBehaviourPoint);
                            if (!character.BattleStatus.HasBehaviourPoint(behaviourPoint))
                            {
                                gameEvents.OnRequestShowMessage.OnNext(new("BPが足りない。", "Sfx.Message.0"));
                                return;
                            }
                            if (!character.BattleStatus.HasStamina(x.NeedStamina))
                            {
                                gameEvents.OnRequestShowMessage.OnNext(new("スタミナが足りない。", "Sfx.Message.0"));
                                return;
                            }
                            source.TrySetResult(new Skill(weapon.ItemId, x.Id, x.CanRegisterUsedSkills));
                            stateMachine.Change(StateNothingAsync);
                        },
                        _ =>
                        {
                            GameTipsView.SetTip(x.Description);
                        });
                });
            });
            var listDocument = GameListView.CreateAsCommand(commandDocumentPrefab, commands, 0);
            listDocument.Q<TMP_Text>("Header").text = "スキルを選べ";
            await scope.WaitUntilCanceled();
            GameTipsView.SetTip(string.Empty);
            if (listDocument != null)
            {
                UnityEngine.Object.Destroy(listDocument.gameObject);
            }
        }

        private async UniTask StateConfirmAsync(CancellationToken scope)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            TinyServiceLocator.Resolve<InputController>().InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    gameEvents.OnRequestPlaySfx.OnNext("Sfx.Cancel.0");
                    stateMachine.Change(StateSelectMainCommandAsync);
                })
                .RegisterTo(scope);
            var commands = new List<Action<HKUIDocument>>
            {
                new(x =>
                {
                    GameListView.ApplyAsSimpleElement(
                        x,
                        "状態異常",
                        _ =>
                        {
                            TinyServiceLocator.Resolve<GameEvents>().OnRequestPlaySfx.OnNext("Sfx.Message.0");
                            stateMachine.Change(StateStatusAsync);
                        });
                }),
            };
            var listDocument = GameListView.CreateAsCommand(commandDocumentPrefab, commands, 0);
            listDocument.Q<TMP_Text>("Header").text = "情報を選べ";
            await scope.WaitUntilCanceled();
            if (listDocument != null)
            {
                UnityEngine.Object.Destroy(listDocument.gameObject);
            }
        }

        private async UniTask StateStatusAsync(CancellationToken scope)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            var informationDocument = UnityEngine.Object.Instantiate(ailmentInformationDocumentPrefab);
            TinyServiceLocator.Resolve<InputController>().InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    gameEvents.OnRequestPlaySfx.OnNext("Sfx.Cancel.0");
                    stateMachine.Change(StateSelectMainCommandAsync);
                })
                .RegisterTo(scope);
            var listElements = character.AilmentController.Elements
                .Select(x =>
                {
                    return new Action<HKUIDocument>(element =>
                    {
                        GameListView.ApplyAsSimpleElement(
                            element,
                            x.GetMasterDataAilment().Name,
                            _ =>
                            {
                            },
                            _ =>
                            {
                                informationDocument.Q<TMP_Text>("Name").text = x.GetMasterDataAilment().Name;
                                informationDocument.Q<TMP_Text>("Description").text = x.GetMasterDataAilment().Description;
                            });
                    });
                });
            var listDocument = GameListView.CreateWithPages(listDocumentPrefab, listElements, 0);
            var isEmpty = !listElements.Any();
            informationDocument.Q("Area.Empty").SetActive(isEmpty);
            informationDocument.Q("Viewport").SetActive(!isEmpty);
            await scope.WaitUntilCanceled();
            if (listDocument != null)
            {
                UnityEngine.Object.Destroy(listDocument.gameObject);
            }
            if (informationDocument != null)
            {
                UnityEngine.Object.Destroy(informationDocument.gameObject);
            }
        }

        private async UniTask StateSelectAllSkillAsync(CancellationToken scope)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            TinyServiceLocator.Resolve<InputController>().InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
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
                                source.TrySetResult(new Skill(Define.TestWeaponId, x.Id, false));
                                stateMachine.Change(StateNothingAsync);
                            },
                            _ =>
                            {
                                GameTipsView.SetTip(x.Description);
                            });
                    });
                });
            var listDocument = GameListView.CreateWithPages(listDocumentPrefab, listElements, 0);
            await scope.WaitUntilCanceled();
            if (listDocument != null)
            {
                UnityEngine.Object.Destroy(listDocument.gameObject);
            }
        }
    }
}
