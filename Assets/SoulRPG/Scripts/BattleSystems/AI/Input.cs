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
        private BattleCharacter actor;

        private BattleCharacter target;

        private BattleCharacter ailmentViewTarget;

        private readonly TinyStateMachine stateMachine = new();

        private readonly HKUIDocument commandDocumentPrefab;

        private readonly HKUIDocument listDocumentPrefab;

        private readonly HKUIDocument ailmentInformationDocumentPrefab;

        private readonly HKUIDocument informationWeaponDocumentPrefab;

        private readonly HKUIDocument informationStatusDocumentPrefab;

        private readonly HKUIDocument informationItemDocumentPrefab;

        private UniTaskCompletionSource<ICommandInvoker> source;

        private int selectedWeaponId;

        private CancellationTokenSource informationWeaponScope;

        private int selectedChangeWeaponEquipmentIndex;

        private int cachedSelectWeaponIndex;

        private int cachedSelectSkillIndex;

        public Input(
            HKUIDocument commandDocumentPrefab,
            HKUIDocument listDocumentPrefab,
            HKUIDocument ailmentInformationDocumentPrefab,
            HKUIDocument informationWeaponDocumentPrefab,
            HKUIDocument informationStatusDocumentPrefab,
            HKUIDocument informationItemDocumentPrefab
            )
        {
            this.listDocumentPrefab = listDocumentPrefab;
            this.commandDocumentPrefab = commandDocumentPrefab;
            this.ailmentInformationDocumentPrefab = ailmentInformationDocumentPrefab;
            this.informationWeaponDocumentPrefab = informationWeaponDocumentPrefab;
            this.informationStatusDocumentPrefab = informationStatusDocumentPrefab;
            this.informationItemDocumentPrefab = informationItemDocumentPrefab;
        }

        public void Dispose()
        {
            stateMachine.Dispose();
        }

        public UniTask<ICommandInvoker> ThinkAsync(BattleCharacter actor, BattleCharacter target)
        {
            this.actor = actor;
            this.target = target;
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
                            AudioManager.PlaySfx("Sfx.Message.0");
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
                            AudioManager.PlaySfx("Sfx.Message.0");
                            stateMachine.Change(StateConfirmAsync);
                        },
                        _ =>
                        {
                            GameTipsView.SetTip("状態異常・ステータスなどの情報を確認する。");
                        });
                }),
                new(x =>
                {
                    GameListView.ApplyAsSimpleElement(
                        x,
                        "武器変更",
                        _ =>
                        {
                            AudioManager.PlaySfx("Sfx.Message.0");
                            stateMachine.Change(StateSelectChangeWeaponEquipmentAsync);
                        },
                        _ =>
                        {
                            GameTipsView.SetTip("装備している武器を変更する。");
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
                                AudioManager.PlaySfx("Sfx.Message.0");
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
            var commands = actor.Equipment.GetWeaponIds()
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
                                if (cachedSelectWeaponIndex != index)
                                {
                                    cachedSelectSkillIndex = 0;
                                }
                                cachedSelectWeaponIndex = index;
                                AudioManager.PlaySfx("Sfx.Message.0");
                                stateMachine.Change(StateSelectSkillAsync);
                            },
                            _ =>
                            {
                                GameTipsView.SetTip(fixedWeaponId.GetMasterDataItem().Description);
                                informationWeaponView.Setup(fixedWeaponId);
                            });
                    });
                });
            var listDocument = GameListView.CreateAsCommand(commandDocumentPrefab, commands, cachedSelectWeaponIndex);
            listDocument.Q<TMP_Text>("Header").text = "武器を選べ";
            TinyServiceLocator.Resolve<InputController>().InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    AudioManager.PlaySfx("Sfx.Cancel.0");
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
                    AudioManager.PlaySfx("Sfx.Cancel.0");
                    stateMachine.Change(StateSelectWeaponAsync);
                })
                .RegisterTo(scope);
            MasterData.Weapon weapon;
            if (!actor.Equipment.GetWeaponId(selectedWeaponId).TryGetMasterDataWeapon(out weapon))
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
                            AudioManager.PlaySfx("Sfx.Message.0");
                            var identifier = Skill.CreateIdentifier(weapon.ItemId, x.Id);
                            if (actor.UsedSkills.Contains(identifier))
                            {
                                gameEvents.OnRequestShowMessage.OnNext(new("このターンではもう使用出来ない。", "Sfx.Message.0"));
                                return;
                            }
                            var behaviourPoint = await actor.GetFixedNeedBehaviourPointAsync(x.NeedBehaviourPoint);
                            if (!actor.BattleStatus.HasBehaviourPoint(behaviourPoint))
                            {
                                gameEvents.OnRequestShowMessage.OnNext(new("BPが足りない。", "Sfx.Message.0"));
                                return;
                            }
                            var stamina = await actor.GetFixedNeedStaminaAsync(x.NeedStamina);
                            if (!actor.BattleStatus.HasStamina(stamina))
                            {
                                gameEvents.OnRequestShowMessage.OnNext(new("スタミナが足りない。", "Sfx.Message.0"));
                                return;
                            }
                            cachedSelectSkillIndex = skills.IndexOf(x);
                            source.TrySetResult(new Skill(weapon.ItemId, x.Id, x.CanRegisterUsedSkills));
                            stateMachine.Change(StateNothingAsync);
                        },
                        _ =>
                        {
                            GameTipsView.SetTip(x.FullDescription());
                        });
                });
            });
            var listDocument = GameListView.CreateAsCommand(commandDocumentPrefab, commands, cachedSelectSkillIndex);
            listDocument.Q<TMP_Text>("Header").text = "スキルを選べ";
            await scope.WaitUntilCanceled();
            GameTipsView.SetTip(string.Empty);
            if (listDocument != null)
            {
                UnityEngine.Object.Destroy(listDocument.gameObject);
            }
            informationWeaponScope?.Cancel();
            informationWeaponScope?.Dispose();
            informationWeaponScope = null;
        }

        private async UniTask StateConfirmAsync(CancellationToken scope)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            TinyServiceLocator.Resolve<InputController>().InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    AudioManager.PlaySfx("Sfx.Cancel.0");
                    stateMachine.Change(StateSelectMainCommandAsync);
                })
                .RegisterTo(scope);
            var commands = new List<Action<HKUIDocument>>
            {
                new(x =>
                {
                    GameListView.ApplyAsSimpleElement(
                        x,
                        "ステータス",
                        _ =>
                        {
                            AudioManager.PlaySfx("Sfx.Message.0");
                            stateMachine.Change(StateStatusAsync);
                        },
                        _ =>
                        {
                            GameTipsView.SetTip("各種ステータスやカット率の確認を行う。");
                        });
                }),
                new(x =>
                {
                    GameListView.ApplyAsSimpleElement(
                        x,
                        "状態異常",
                        _ =>
                        {
                            AudioManager.PlaySfx("Sfx.Message.0");
                            ailmentViewTarget = actor;
                            stateMachine.Change(StateAilmentAsync);
                        },
                        _ =>
                        {
                            GameTipsView.SetTip("自分自身に付与されている状態異常を確認する。");
                        });
                }),
            };
            if (actor.BattleStatus.CanViewTargetAilment)
            {
                commands.Add(
                    x =>
                    {
                        GameListView.ApplyAsSimpleElement(
                            x,
                            "状態異常（敵）",
                            _ =>
                            {
                                AudioManager.PlaySfx("Sfx.Message.0");
                                ailmentViewTarget = target;
                                stateMachine.Change(StateAilmentAsync);
                            },
                            _ =>
                            {
                                GameTipsView.SetTip("敵に付与されている状態異常を確認する。");
                            });
                    }
                );
            }
            var listDocument = GameListView.CreateAsCommand(commandDocumentPrefab, commands, 0);
            listDocument.Q<TMP_Text>("Header").text = "情報を選べ";
            await scope.WaitUntilCanceled();
            if (listDocument != null)
            {
                UnityEngine.Object.Destroy(listDocument.gameObject);
            }
        }

        private UniTask StateStatusAsync(CancellationToken scope)
        {
            GameStatusInformationView.Open(informationStatusDocumentPrefab, actor.Character, scope);
            TinyServiceLocator.Resolve<InputController>().InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    AudioManager.PlaySfx("Sfx.Cancel.0");
                    stateMachine.Change(StateSelectMainCommandAsync);
                })
                .RegisterTo(scope);
            return UniTask.CompletedTask;
        }

        private async UniTask StateAilmentAsync(CancellationToken scope)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            var informationDocument = UnityEngine.Object.Instantiate(ailmentInformationDocumentPrefab);
            TinyServiceLocator.Resolve<InputController>().InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    AudioManager.PlaySfx("Sfx.Cancel.0");
                    stateMachine.Change(StateSelectMainCommandAsync);
                })
                .RegisterTo(scope);
            var listElements = ailmentViewTarget.AilmentController.Elements
                .Select(x =>
                {
                    return new Action<HKUIDocument>(element =>
                    {
                        var header = x.GetRemainingTurnCount() <= -1
                            ? x.GetMasterDataAilment().Name
                            : $"{x.GetMasterDataAilment().Name}(残り{x.GetRemainingTurnCount()}ターン)";
                        GameListView.ApplyAsSimpleElement(
                            element,
                            header,
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
                    AudioManager.PlaySfx("Sfx.Cancel.0");
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
                                GameTipsView.SetTip(x.FullDescription());
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

        private async UniTask StateSelectChangeWeaponEquipmentAsync(CancellationToken scope)
        {
            informationWeaponScope?.Cancel();
            informationWeaponScope?.Dispose();
            informationWeaponScope = new CancellationTokenSource();
            var informationWeaponView = new BattleInformationWeaponView(informationWeaponDocumentPrefab, informationWeaponScope.Token);
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            var commands = actor.Equipment.GetWeaponIds()
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
                                selectedChangeWeaponEquipmentIndex = index;
                                AudioManager.PlaySfx("Sfx.Message.0");
                                stateMachine.Change(StateSelectChangeWeaponInventoryAsync);
                            },
                            _ =>
                            {
                                GameTipsView.SetTip(fixedWeaponId.GetMasterDataItem().Description);
                                informationWeaponView.Setup(fixedWeaponId);
                            });
                    });
                });
            var listDocument = GameListView.CreateAsCommand(commandDocumentPrefab, commands, 0);
            listDocument.Q<TMP_Text>("Header").text = "変更する武器を選べ";
            TinyServiceLocator.Resolve<InputController>().InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    AudioManager.PlaySfx("Sfx.Cancel.0");
                    stateMachine.Change(StateSelectMainCommandAsync);
                })
                .RegisterTo(scope);
            await scope.WaitUntilCanceled();
            informationWeaponScope?.Cancel();
            informationWeaponScope?.Dispose();
            informationWeaponScope = null;
            if (listDocument != null)
            {
                UnityEngine.Object.Destroy(listDocument.gameObject);
            }
        }

        private async UniTask StateSelectChangeWeaponInventoryAsync(CancellationToken scope)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            var informationItemView = new GameItemInformationView(informationItemDocumentPrefab, scope);
            TinyServiceLocator.Resolve<InputController>().InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    AudioManager.PlaySfx("Sfx.Cancel.0");
                    stateMachine.Change(StateSelectChangeWeaponEquipmentAsync);
                })
                .RegisterTo(scope);
            var listElements = actor.Character.Inventory.Items
                .Where(x => !actor.Equipment.GetWeaponIds().Any(y => y == x.Key) && x.Key.TryGetMasterDataWeapon(out _))
                .Select(x =>
                {
                    return new Action<HKUIDocument>(element =>
                    {
                        GameListView.ApplyAsSimpleElement(
                            element,
                            x.Key.GetMasterDataItem().Name,
                            _ =>
                            {
                                source.TrySetResult(new ChangeWeapon(selectedChangeWeaponEquipmentIndex, x.Key));
                                stateMachine.Change(StateNothingAsync);
                            },
                            _ =>
                            {
                                informationItemView.Setup(x.Key.GetMasterDataItem());
                            });
                    });
                });
            var listDocument = GameListView.CreateWithPages(listDocumentPrefab, listElements, 0);
            var isEmpty = !listElements.Any();
            await scope.WaitUntilCanceled();
            if (listDocument != null)
            {
                UnityEngine.Object.Destroy(listDocument.gameObject);
            }
        }
    }
}
