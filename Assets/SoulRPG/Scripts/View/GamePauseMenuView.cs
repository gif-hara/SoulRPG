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
                            "装備変更",
                            _ =>
                            {
                                stateMachine.Change(StateSelectEquipmentPartAsync);
                                AudioManager.PlaySFX("Sfx.Message.0");
                            },
                            _ =>
                            {
                                GameTipsView.SetTip("戦闘で使う装備を変更する。");
                            }
                        );
                    },
                    element =>
                    {
                        GameListView.ApplyAsSimpleElement
                        (
                            element,
                            "システム",
                            _ =>
                            {
                                stateMachine.Change(StateOptionsAsync);
                                AudioManager.PlaySFX("Sfx.Message.0");
                            },
                            _ =>
                            {
                                GameTipsView.SetTip("システム設定を行う。");
                            }
                        );
                    },
                    element =>
                    {
                        GameListView.ApplyAsSimpleElement
                        (
                            element,
                            "中断データ作成",
                            _ =>
                            {
                                stateMachine.Change(StateCreateSuspendData);
                                AudioManager.PlaySFX("Sfx.Message.0");
                            },
                            _ =>
                            {
                                GameTipsView.SetTip("中断データを作成してゲームを終了します。");
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
                        AudioManager.PlaySFX("Sfx.Cancel.0");
                    })
                    .RegisterTo(scope);
            CreateHeader("メニュー", scope);
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
                var weaponName = x == 0 ? "なし" : x.GetMasterDataItem().Name;
                return new Action<HKUIDocument>(element =>
                {
                    GameListView.ApplyAsSimpleElement(element, $"武器{i + 1}: {weaponName}", _ =>
                    {
                        context = new EquipmentChangeController(character, (EquipmentChangeController.PartType)i + (int)EquipmentChangeController.PartType.Weapon1);
                        stateMachine.Change(StateSelectWeaponAsync);
                        cachedStateSelectEquipmentPartIndex = i;
                        AudioManager.PlaySFX("Sfx.Message.0");
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
                GameListView.ApplyAsSimpleElement(element, $"頭: {(character.Equipment.HeadId == 0 ? "なし" : character.Equipment.HeadId.GetMasterDataItem().Name)}", _ =>
                {
                    context = new EquipmentChangeController(character, EquipmentChangeController.PartType.Head);
                    stateMachine.Change(StateSelectArmorHeadAsync);
                    cachedStateSelectEquipmentPartIndex = 4;
                    AudioManager.PlaySFX("Sfx.Message.0");
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
                GameListView.ApplyAsSimpleElement(element, $"胴: {(character.Equipment.BodyId == 0 ? "なし" : character.Equipment.BodyId.GetMasterDataItem().Name)}", _ =>
                {
                    context = new EquipmentChangeController(character, EquipmentChangeController.PartType.Body);
                    stateMachine.Change(StateSelectArmorBodyAsync);
                    cachedStateSelectEquipmentPartIndex = 5;
                    AudioManager.PlaySFX("Sfx.Message.0");
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
                GameListView.ApplyAsSimpleElement(element, $"腕: {(character.Equipment.ArmId == 0 ? "なし" : character.Equipment.ArmId.GetMasterDataItem().Name)}", _ =>
                {
                    context = new EquipmentChangeController(character, EquipmentChangeController.PartType.Arm);
                    stateMachine.Change(StateSelectArmorArmsAsync);
                    cachedStateSelectEquipmentPartIndex = 6;
                    AudioManager.PlaySFX("Sfx.Message.0");
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
                GameListView.ApplyAsSimpleElement(element, $"脚: {(character.Equipment.LegId == 0 ? "なし" : character.Equipment.LegId.GetMasterDataItem().Name)}", _ =>
                {
                    context = new EquipmentChangeController(character, EquipmentChangeController.PartType.Leg);
                    stateMachine.Change(StateSelectArmorLegsAsync);
                    cachedStateSelectEquipmentPartIndex = 7;
                    AudioManager.PlaySFX("Sfx.Message.0");
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
                var accessoryName = x == 0 ? "なし" : x.GetMasterDataItem().Name;
                return new Action<HKUIDocument>(element =>
                {
                    GameListView.ApplyAsSimpleElement(element, $"アクセサリ{i + 1}: {accessoryName}", _ =>
                    {
                        context = new EquipmentChangeController(character, (EquipmentChangeController.PartType)i + (int)EquipmentChangeController.PartType.Accessory1);
                        stateMachine.Change(StateSelectAccessoryAsync);
                        cachedStateSelectEquipmentPartIndex = i + 8;
                        AudioManager.PlaySFX("Sfx.Message.0");
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
                    AudioManager.PlaySFX("Sfx.Cancel.0");
                })
                .RegisterTo(scope);
            CreateHeader("装備変更", scope);
            GameTipsView.SetTip("変更したい部位を選択せよ。");
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
                        x.Key.GetMasterDataItem().Name, _ =>
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
                    AudioManager.PlaySFX("Sfx.Cancel.0");
                })
                .RegisterTo(scope);
            CreateHeader("武器変更", scope);
            GameTipsView.SetTip("装備したい武器を選択せよ。");
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
                        x.Key.GetMasterDataItem().Name, _ =>
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
                    AudioManager.PlaySFX("Sfx.Cancel.0");
                })
                .RegisterTo(scope);
            CreateHeader("頭防具変更", scope);
            GameTipsView.SetTip("装備したい頭防具を選択せよ。");
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
                        x.Key.GetMasterDataItem().Name, _ =>
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
                    AudioManager.PlaySFX("Sfx.Cancel.0");
                })
                .RegisterTo(scope);
            CreateHeader("胴防具変更", scope);
            GameTipsView.SetTip("装備したい胴防具を選択せよ。");
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
                        x.Key.GetMasterDataItem().Name, _ =>
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
                    AudioManager.PlaySFX("Sfx.Cancel.0");
                })
                .RegisterTo(scope);
            CreateHeader("腕防具変更", scope);
            GameTipsView.SetTip("装備したい腕防具を選択せよ。");
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
                        x.Key.GetMasterDataItem().Name, _ =>
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
                    AudioManager.PlaySFX("Sfx.Cancel.0");
                })
                .RegisterTo(scope);
            CreateHeader("脚防具変更", scope);
            GameTipsView.SetTip("装備したい脚防具を選択せよ。");
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
                        x.Key.GetMasterDataItem().Name, _ =>
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
                    AudioManager.PlaySFX("Sfx.Cancel.0");
                })
                .RegisterTo(scope);
            CreateHeader("アクセサリー変更", scope);
            GameTipsView.SetTip("装備したいアクセサリーを選択せよ。");
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
                "中断データを作成してゲームを終了しますか？",
                new[] { "はい", "いいえ" },
                0,
                scope
                );
            if (index == 0)
            {
                var saveData = SaveData.Load();
                if (saveData == null)
                {
                    saveData = new SaveData();
                }
                saveData.suspendData = new SaveData.SuspendData
                {
                    growthParameter = character.GrowthParameter,
                    playerPosition = character.Position,
                    playerDirection = character.Direction,
                    playerEquipmentData = character.Equipment.CreateSaveData(),
                    playerItemData = character.Inventory.CreateSaveData()
                };
                SaveData.Save(saveData);
                stateMachine.Change(StateCreatedSuspendData);
            }
            else
            {
                stateMachine.Change(StateRootMenuAsync);
            }
        }

        private async UniTask StateCreatedSuspendData(CancellationToken scope)
        {
            await DialogView.ConfirmAsync
            (
                documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Dialog"),
                "中断データを作成しました。このままゲームを終了できます。お疲れ様でした。",
                new string[] { },
                0,
                scope
            );
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
