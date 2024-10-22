using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.CharacterControllers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GamePauseMenuView
    {
        private readonly HKUIDocument documentBundlePrefab;

        private readonly Character character;

        private readonly TinyStateMachine stateMachine;

        private readonly UniTaskCompletionSource openCompletionSource = new();

        private readonly InputController inputController;

        private IContext context;

        private int cachedStateSelectEquipmentPartIndex;

        public GamePauseMenuView(HKUIDocument documentBundlePrefab, Character character)
        {
            this.documentBundlePrefab = documentBundlePrefab;
            this.character = character;
            this.stateMachine = new TinyStateMachine();
            inputController = TinyServiceLocator.Resolve<InputController>();
        }

        public async UniTask OpenAsync()
        {
            inputController.PushInputType(InputController.InputType.UI);
            stateMachine.Change(StateRootMenuAsync);
            var explorationView = TinyServiceLocator.Resolve<ExplorationView>();
            explorationView.SetActive(false);
            await openCompletionSource.Task;
            inputController.PopInputType();
            explorationView.SetActive(true);
        }

        private async UniTask StateRootMenuAsync(CancellationToken scope)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            var listDocument = CreateList(new List<Action<HKUIDocument>>
                {
                    element =>
                    {
                        GameListView.ApplyAsSimpleElement
                        (
                            element,
                            "装備変更".Localized(),
                            _ =>
                            {
                                stateMachine.Change(StateSelectEquipmentPartAsync);
                                AudioManager.PlaySfx("Sfx.Message.0");
                            },
                            _ =>
                            {
                                GameTipsView.SetTip("戦闘で使う装備を変更する。".Localized());
                            }
                        );
                    },
                    element =>
                    {
                        GameListView.ApplyAsSimpleElement
                        (
                            element,
                            "システム".Localized(),
                            _ =>
                            {
                                stateMachine.Change(StateOptionsAsync);
                                AudioManager.PlaySfx("Sfx.Message.0");
                            },
                            _ =>
                            {
                                GameTipsView.SetTip("システム設定を行う。".Localized());
                            }
                        );
                    },
                    element =>
                    {
                        GameListView.ApplyAsSimpleElement
                        (
                            element,
                            "中断データ作成".Localized(),
                            _ =>
                            {
                                stateMachine.Change(StateCreateSuspendData);
                                AudioManager.PlaySfx("Sfx.Message.0");
                            },
                            _ =>
                            {
                                GameTipsView.SetTip("中断データを作成してゲームを終了します。".Localized());
                            }
                        );
                    }
                },
                0
            );

            inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                    .Subscribe(_ =>
                    {
                        stateMachine.Change(StateCloseAsync);
                        AudioManager.PlaySfx("Sfx.Cancel.0");
                    })
                    .RegisterTo(scope);
            CreateHeader("メニュー".Localized(), scope);
            GameStatusInformationView.Open(documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Info.Status"), character, scope);
            cachedStateSelectEquipmentPartIndex = 0;
            await UniTask.WaitUntilCanceled(scope);
            Object.Destroy(listDocument.gameObject);
        }

        private async UniTask StateSelectEquipmentPartAsync(CancellationToken scope)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            var gameItemInformationView = new GameItemInformationView(documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Info.Item"), scope);
            var weaponElements = character.Equipment.GetWeaponIds().Select((x, i) =>
            {
                var weaponName = x == 0 ? "なし" : x.GetMasterDataItem().Name.Localized();
                return new Action<HKUIDocument>(element =>
                {
                    var message = "武器{0}: {1}".Localized().Format(i + 1, weaponName.Localized());
                    GameListView.ApplyAsSimpleElement(element, message, _ =>
                    {
                        context = new EquipmentChangeController(character, (EquipmentChangeController.PartType)i + (int)EquipmentChangeController.PartType.Weapon1);
                        stateMachine.Change(StateSelectWeaponAsync);
                        cachedStateSelectEquipmentPartIndex = i;
                        AudioManager.PlaySfx("Sfx.Message.0");
                    },
                    _ =>
                    {
                        if (x != 0)
                        {
                            gameItemInformationView.Setup(x.GetMasterDataItem());
                        }
                        else
                        {
                            gameItemInformationView.SetupAsEmpty();
                        }
                    });
                });
            });
            var headElement = new Action<HKUIDocument>(element =>
            {
                var headId = character.Equipment.HeadId;
                var message = "頭: {0}".Localized().Format(headId == 0 ? "なし".Localized() : headId.GetMasterDataItem().Name.Localized());
                GameListView.ApplyAsSimpleElement(element, message, _ =>
                {
                    context = new EquipmentChangeController(character, EquipmentChangeController.PartType.Head);
                    stateMachine.Change(StateSelectArmorHeadAsync);
                    cachedStateSelectEquipmentPartIndex = 4;
                    AudioManager.PlaySfx("Sfx.Message.0");
                },
                _ =>
                {
                    if (character.Equipment.HeadId != 0)
                    {
                        gameItemInformationView.Setup(character.Equipment.HeadId.GetMasterDataItem());
                    }
                    else
                    {
                        gameItemInformationView.SetupAsEmpty();
                    }
                });
            });
            var bodyElement = new Action<HKUIDocument>(element =>
            {
                var bodyId = character.Equipment.BodyId;
                var message = "胴: {0}".Localized().Format(bodyId == 0 ? "なし".Localized() : bodyId.GetMasterDataItem().Name.Localized());
                GameListView.ApplyAsSimpleElement(element, message, _ =>
                {
                    context = new EquipmentChangeController(character, EquipmentChangeController.PartType.Body);
                    stateMachine.Change(StateSelectArmorBodyAsync);
                    cachedStateSelectEquipmentPartIndex = 5;
                    AudioManager.PlaySfx("Sfx.Message.0");
                },
                _ =>
                {
                    if (character.Equipment.BodyId != 0)
                    {
                        gameItemInformationView.Setup(character.Equipment.BodyId.GetMasterDataItem());
                    }
                    else
                    {
                        gameItemInformationView.SetupAsEmpty();
                    }
                });
            });
            var armElement = new Action<HKUIDocument>(element =>
            {
                var armdId = character.Equipment.ArmId;
                var message = "腕: {0}".Localized().Format(armdId == 0 ? "なし".Localized() : armdId.GetMasterDataItem().Name.Localized());
                GameListView.ApplyAsSimpleElement(element, message, _ =>
                {
                    context = new EquipmentChangeController(character, EquipmentChangeController.PartType.Arm);
                    stateMachine.Change(StateSelectArmorArmsAsync);
                    cachedStateSelectEquipmentPartIndex = 6;
                    AudioManager.PlaySfx("Sfx.Message.0");
                },
                _ =>
                {
                    if (character.Equipment.ArmId != 0)
                    {
                        gameItemInformationView.Setup(character.Equipment.ArmId.GetMasterDataItem());
                    }
                    else
                    {
                        gameItemInformationView.SetupAsEmpty();
                    }
                });
            });
            var legElement = new Action<HKUIDocument>(element =>
            {
                var legId = character.Equipment.LegId;
                var message = "脚: {0}".Localized().Format(legId == 0 ? "なし".Localized() : legId.GetMasterDataItem().Name.Localized());
                GameListView.ApplyAsSimpleElement(element, message, _ =>
                {
                    context = new EquipmentChangeController(character, EquipmentChangeController.PartType.Leg);
                    stateMachine.Change(StateSelectArmorLegsAsync);
                    cachedStateSelectEquipmentPartIndex = 7;
                    AudioManager.PlaySfx("Sfx.Message.0");
                },
                _ =>
                {
                    if (character.Equipment.LegId != 0)
                    {
                        gameItemInformationView.Setup(character.Equipment.LegId.GetMasterDataItem());
                    }
                    else
                    {
                        gameItemInformationView.SetupAsEmpty();
                    }
                });
            });
            var accessoryElements = character.Equipment.GetAccessoryIds().Select((x, i) =>
            {
                var accessoryName = x == 0 ? "なし" : x.GetMasterDataItem().Name.Localized();
                return new Action<HKUIDocument>(element =>
                {
                    var message = "アクセサリー{0}: {1}".Localized().Format(i + 1, accessoryName.Localized());
                    GameListView.ApplyAsSimpleElement(element, message, _ =>
                    {
                        context = new EquipmentChangeController(character, (EquipmentChangeController.PartType)i + (int)EquipmentChangeController.PartType.Accessory1);
                        stateMachine.Change(StateSelectAccessoryAsync);
                        cachedStateSelectEquipmentPartIndex = i + 8;
                        AudioManager.PlaySfx("Sfx.Message.0");
                    },
                    _ =>
                    {
                        if (x != 0)
                        {
                            gameItemInformationView.Setup(x.GetMasterDataItem());
                        }
                        else
                        {
                            gameItemInformationView.SetupAsEmpty();
                        }
                    });
                });
            });
            var listElements = new List<Action<HKUIDocument>>();
            listElements.AddRange(weaponElements);
            listElements.Add(headElement);
            listElements.Add(bodyElement);
            listElements.Add(armElement);
            listElements.Add(legElement);
            listElements.AddRange(accessoryElements);

            var listDocument = GameListView.CreateWithPages
            (
                documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.List"),
                listElements,
                cachedStateSelectEquipmentPartIndex
            );
            inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(StateRootMenuAsync);
                    AudioManager.PlaySfx("Sfx.Cancel.0");
                })
                .RegisterTo(scope);
            CreateHeader("装備変更".Localized(), scope);
            GameTipsView.SetTip("変更したい部位を選択せよ。".Localized());
            await UniTask.WaitUntilCanceled(scope);
            Object.Destroy(listDocument.gameObject);
        }

        private async UniTask StateSelectWeaponAsync(CancellationToken scope)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            var gameItemInformationView = new GameItemInformationView(documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Info.Item"), scope);
            var listElements = character.Inventory.Items
                .Where(x => x.Key.ContainsMasterDataWeapon())
                .Select(x =>
                {
                    return new Action<HKUIDocument>(element =>
                    {
                        GameListView.ApplyAsSimpleElement(element,
                        x.Key.GetMasterDataItem().Name.Localized(), _ =>
                        {
                            var equipmentChangeController = (EquipmentChangeController)context;
                            equipmentChangeController.ChangeEquipment(x.Key);
                            stateMachine.Change(StateSelectEquipmentPartAsync);
                        },
                        _ =>
                        {
                            gameItemInformationView.Setup(x.Key.GetMasterDataItem());
                        });
                    });
                });
            var listDocument = CreateList
            (
                listElements,
                0
            );
            if (!listElements.Any())
            {
                gameItemInformationView.SetupAsEmpty();
            }
            inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(StateSelectEquipmentPartAsync);
                    AudioManager.PlaySfx("Sfx.Cancel.0");
                })
                .RegisterTo(scope);
            CreateHeader("武器変更".Localized(), scope);
            GameTipsView.SetTip("装備したい武器を選択せよ。".Localized());
            await UniTask.WaitUntilCanceled(scope);
            Object.Destroy(listDocument.gameObject);
        }

        private async UniTask StateSelectArmorHeadAsync(CancellationToken scope)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            var gameItemInformationView = new GameItemInformationView(documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Info.Item"), scope);
            var listElements = character.Inventory.Items
                .Where(x => x.Key.ContainsMasterDataArmorHead())
                .Select(x =>
                {
                    return new Action<HKUIDocument>(element =>
                    {
                        GameListView.ApplyAsSimpleElement(element,
                        x.Key.GetMasterDataItem().Name.Localized(), _ =>
                        {
                            var equipmentChangeController = (EquipmentChangeController)context;
                            equipmentChangeController.ChangeEquipment(x.Key);
                            stateMachine.Change(StateSelectEquipmentPartAsync);
                        },
                        _ =>
                        {
                            gameItemInformationView.Setup(x.Key.GetMasterDataItem());
                        });
                    });
                });
            var listDocument = CreateList(listElements, 0);
            if (!listElements.Any())
            {
                gameItemInformationView.SetupAsEmpty();
            }
            inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(StateSelectEquipmentPartAsync);
                    AudioManager.PlaySfx("Sfx.Cancel.0");
                })
                .RegisterTo(scope);
            CreateHeader("頭防具変更".Localized(), scope);
            GameTipsView.SetTip("装備したい頭防具を選択せよ。".Localized());
            await UniTask.WaitUntilCanceled(scope);
            Object.Destroy(listDocument.gameObject);
        }

        private async UniTask StateSelectArmorBodyAsync(CancellationToken scope)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            var gameItemInformationView = new GameItemInformationView(documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Info.Item"), scope);
            var listElements = character.Inventory.Items
                .Where(x => x.Key.ContainsMasterDataArmorBody())
                .Select(x =>
                {
                    return new Action<HKUIDocument>(element =>
                    {
                        GameListView.ApplyAsSimpleElement(element,
                        x.Key.GetMasterDataItem().Name.Localized(), _ =>
                        {
                            var equipmentChangeController = (EquipmentChangeController)context;
                            equipmentChangeController.ChangeEquipment(x.Key);
                            stateMachine.Change(StateSelectEquipmentPartAsync);
                        },
                        _ =>
                        {
                            gameItemInformationView.Setup(x.Key.GetMasterDataItem());
                        });
                    });
                });
            var listDocument = CreateList(listElements, 0);
            if (!listElements.Any())
            {
                gameItemInformationView.SetupAsEmpty();
            }
            inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(StateSelectEquipmentPartAsync);
                    AudioManager.PlaySfx("Sfx.Cancel.0");
                })
                .RegisterTo(scope);
            CreateHeader("胴防具変更".Localized(), scope);
            GameTipsView.SetTip("装備したい胴防具を選択せよ。".Localized());
            await UniTask.WaitUntilCanceled(scope);
            Object.Destroy(listDocument.gameObject);
        }

        private async UniTask StateSelectArmorArmsAsync(CancellationToken scope)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            var gameItemInformationView = new GameItemInformationView(documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Info.Item"), scope);
            var listElements = character.Inventory.Items
                .Where(x => x.Key.ContainsMasterDataArmorArms())
                .Select(x =>
                {
                    return new Action<HKUIDocument>(element =>
                    {
                        GameListView.ApplyAsSimpleElement(element,
                        x.Key.GetMasterDataItem().Name.Localized(), _ =>
                        {
                            var equipmentChangeController = (EquipmentChangeController)context;
                            equipmentChangeController.ChangeEquipment(x.Key);
                            stateMachine.Change(StateSelectEquipmentPartAsync);
                        },
                        _ =>
                        {
                            gameItemInformationView.Setup(x.Key.GetMasterDataItem());
                        });
                    });
                });
            var listDocument = CreateList(listElements, 0);
            if (!listElements.Any())
            {
                gameItemInformationView.SetupAsEmpty();
            }
            inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(StateSelectEquipmentPartAsync);
                    AudioManager.PlaySfx("Sfx.Cancel.0");
                })
                .RegisterTo(scope);
            CreateHeader("腕防具変更".Localized(), scope);
            GameTipsView.SetTip("装備したい腕防具を選択せよ。".Localized());
            await UniTask.WaitUntilCanceled(scope);
            Object.Destroy(listDocument.gameObject);
        }

        private async UniTask StateSelectArmorLegsAsync(CancellationToken scope)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            var gameItemInformationView = new GameItemInformationView(documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Info.Item"), scope);
            var listElements = character.Inventory.Items
                .Where(x => x.Key.ContainsMasterDataArmorLegs())
                .Select(x =>
                {
                    return new Action<HKUIDocument>(element =>
                    {
                        GameListView.ApplyAsSimpleElement(element,
                        x.Key.GetMasterDataItem().Name.Localized(), _ =>
                        {
                            var equipmentChangeController = (EquipmentChangeController)context;
                            equipmentChangeController.ChangeEquipment(x.Key);
                            stateMachine.Change(StateSelectEquipmentPartAsync);
                        },
                        _ =>
                        {
                            gameItemInformationView.Setup(x.Key.GetMasterDataItem());
                        });
                    });
                });
            var listDocument = CreateList(listElements, 0);
            if (!listElements.Any())
            {
                gameItemInformationView.SetupAsEmpty();
            }
            inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(StateSelectEquipmentPartAsync);
                    AudioManager.PlaySfx("Sfx.Cancel.0");
                })
                .RegisterTo(scope);
            CreateHeader("脚防具変更".Localized(), scope);
            GameTipsView.SetTip("装備したい脚防具を選択せよ。".Localized());
            await UniTask.WaitUntilCanceled(scope);
            Object.Destroy(listDocument.gameObject);
        }

        private async UniTask StateSelectAccessoryAsync(CancellationToken scope)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            var gameItemInformationView = new GameItemInformationView(documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Info.Item"), scope);
            var listElements = character.Inventory.Items
                .Where(x => x.Key.ContainsMasterDataAccessory())
                .Select(x =>
                {
                    return new Action<HKUIDocument>(element =>
                    {
                        GameListView.ApplyAsSimpleElement(element,
                        x.Key.GetMasterDataItem().Name.Localized(), _ =>
                        {
                            var equipmentChangeController = (EquipmentChangeController)context;
                            equipmentChangeController.ChangeEquipment(x.Key);
                            stateMachine.Change(StateSelectEquipmentPartAsync);
                        },
                        _ =>
                        {
                            gameItemInformationView.Setup(x.Key.GetMasterDataItem());
                        });
                    });
                });
            var listDocument = CreateList(listElements, 0);
            if (!listElements.Any())
            {
                gameItemInformationView.SetupAsEmpty();
            }
            inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(StateSelectEquipmentPartAsync);
                    AudioManager.PlaySfx("Sfx.Cancel.0");
                })
                .RegisterTo(scope);
            CreateHeader("アクセサリー変更".Localized(), scope);
            GameTipsView.SetTip("装備したいアクセサリーを選択せよ。".Localized());
            await UniTask.WaitUntilCanceled(scope);
            Object.Destroy(listDocument.gameObject);
        }

        private async UniTask StateOptionsAsync(CancellationToken scope)
        {
            await OptionsView.OpenAsync(documentBundlePrefab, scope);
            stateMachine.Change(StateRootMenuAsync);
        }

        private async UniTask StateCreateSuspendData(CancellationToken scope)
        {
            var index = await DialogView.ConfirmAsync(
                documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Dialog"),
                "中断データを作成してゲームを終了しますか？".Localized(),
                new[] { "はい".Localized(), "いいえ".Localized() },
                0,
                scope
                );
            if (index == 0)
            {
                var suspendData = new SuspendData
                {
                    growthParameter = character.GrowthParameter,
                    playerPosition = character.Position,
                    playerDirection = character.Direction,
                    playerEquipmentData = character.Equipment.CreateSuspendData(),
                    playerItemData = character.Inventory.CreateSuspendData(),
                    playerInstanceData = character.InstanceStatus.CreateSuspendData(),
                    dungeonData = TinyServiceLocator.Resolve<DungeonController>().CreateSuspendData()
                };
                suspendData.Save();
                stateMachine.Change(StateCreatedSuspendData);
            }
            else
            {
                stateMachine.Change(StateRootMenuAsync);
            }
        }

        private async UniTask StateCreatedSuspendData(CancellationToken scope)
        {
#if UNITY_WEBGL
            await DialogView.ConfirmAsync
            (
                documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Dialog"),
                "中断データを作成しました。このままゲームを終了できます。お疲れ様でした。".Localized(),
                new string[] { },
                0,
                scope
            );
#else
            await DialogView.ConfirmAsync
            (
                documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Dialog"),
                "中断データを作成しました。このままゲームを終了できます。お疲れ様でした。".Localized(),
                new[] { "終了".Localized() },
                0,
                scope
            );
            Application.Quit();
#endif
        }

        private UniTask StateCloseAsync(CancellationToken scope)
        {
            GameTipsView.SetTip(string.Empty);
            openCompletionSource.TrySetResult();
            return UniTask.CompletedTask;
        }

        private HKUIDocument CreateList
        (
            IEnumerable<Action<HKUIDocument>> elements,
            int initialElement
        )
        {
            return GameListView.CreateWithPages
            (
                documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.List"),
                elements,
                initialElement
            );
        }

        private HKUIDocument CreateHeader(string header, CancellationToken scope)
        {
            return HeaderView.Open(documentBundlePrefab.Q<HKUIDocument>("UI.Game.Header"), header, scope);
        }

        public interface IContext
        {
        }
    }
}
