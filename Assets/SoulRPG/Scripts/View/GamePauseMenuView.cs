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

        public GamePauseMenuView(HKUIDocument documentBundlePrefab, Character character)
        {
            this.documentBundlePrefab = documentBundlePrefab;
            this.character = character;
            this.stateMachine = new TinyStateMachine();
            inputController = TinyServiceLocator.Resolve<InputController>();
        }

        public async UniTask OpenAsync()
        {
            inputController.ChangeInputType(InputController.InputType.UI);
            stateMachine.Change(StateRootMenuAsync);
            await openCompletionSource.Task;
            inputController.ChangeInputType(InputController.InputType.InGame);
        }

        private async UniTask StateRootMenuAsync(CancellationToken scope)
        {
            var listDocument = CreateList(new List<Action<HKUIDocument>>
                {
                    element =>
                    {
                        GameListView.ApplyAsSimpleElement
                        (
                            element,
                            "装備",
                            _ => { stateMachine.Change(StateSelectEquipmentPartAsync); }
                        );
                    },
                    element =>
                    {
                        GameListView.ApplyAsSimpleElement
                        (
                            element,
                            "道具",
                            _ => { Debug.Log("道具"); }
                        );
                    },
                    element =>
                    {
                        GameListView.ApplyAsSimpleElement
                        (
                            element,
                            "ステータス",
                            _ => { Debug.Log("ステータス"); }
                        );
                    },
                    element =>
                    {
                        GameListView.ApplyAsSimpleElement
                        (
                            element,
                            "システム",
                            _ => { Debug.Log("システム"); }
                        );
                    },
                },
                0
            );

        inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(StateCloseAsync);
                })
                .RegisterTo(scope);
            await UniTask.WaitUntilCanceled(scope);
            Object.Destroy(listDocument.gameObject);
        }

        private async UniTask StateSelectEquipmentPartAsync(CancellationToken scope)
        {
            var weaponElements = character.Equipment.GetWeaponIds().Select((x, i) =>
            {
                var weaponName = x == 0 ? "なし" : x.GetMasterDataItem().Name;
                return new Action<HKUIDocument>(element =>
                {
                    GameListView.ApplyAsSimpleElement(element, $"武器{i + 1}: {weaponName}", _ =>
                    {
                        context = new EquipmentChangeController(character, (EquipmentChangeController.PartType)i + (int)EquipmentChangeController.PartType.Weapon1);
                        stateMachine.Change(StateSelectWeaponAsync);
                    });
                });
            });
            var headElement = new Action<HKUIDocument>(element =>
            {
                GameListView.ApplyAsSimpleElement(element, $"頭: {(character.Equipment.HeadId == 0 ? "なし" : character.Equipment.HeadId.GetMasterDataItem().Name)}", _ =>
                {
                    context = new EquipmentChangeController(character, EquipmentChangeController.PartType.Head);
                    stateMachine.Change(StateSelectArmorHeadAsync);
                });
            });
            var bodyElement = new Action<HKUIDocument>(element =>
            {
                GameListView.ApplyAsSimpleElement(element, $"胴: {(character.Equipment.BodyId == 0 ? "なし" : character.Equipment.BodyId.GetMasterDataItem().Name)}", _ =>
                {
                    context = new EquipmentChangeController(character, EquipmentChangeController.PartType.Body);
                    stateMachine.Change(StateSelectArmorBodyAsync);
                });
            });
            var armElement = new Action<HKUIDocument>(element =>
            {
                GameListView.ApplyAsSimpleElement(element, $"腕: {(character.Equipment.ArmId == 0 ? "なし" : character.Equipment.ArmId.GetMasterDataItem().Name)}", _ =>
                {
                    context = new EquipmentChangeController(character, EquipmentChangeController.PartType.Arm);
                    stateMachine.Change(StateSelectArmorArmsAsync);
                });
            });
            var legElement = new Action<HKUIDocument>(element =>
            {
                GameListView.ApplyAsSimpleElement(element, $"脚: {(character.Equipment.LegId == 0 ? "なし" : character.Equipment.LegId.GetMasterDataItem().Name)}", _ =>
                {
                    context = new EquipmentChangeController(character, EquipmentChangeController.PartType.Leg);
                    stateMachine.Change(StateSelectArmorLegsAsync);
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

            var listDocument = GameListView.Create
            (
                documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.List"),
                listElements,
                0
            );
            inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(StateRootMenuAsync);
                })
                .RegisterTo(scope);
            await UniTask.WaitUntilCanceled(scope);
            Object.Destroy(listDocument.gameObject);
        }

        private async UniTask StateSelectWeaponAsync(CancellationToken scope)
        {
            var listElements = character.Inventory.Items
                .Where(x => x.Key.ContainsMasterDataWeapon())
                .Select(x =>
                {
                    return new Action<HKUIDocument>(element =>
                    {
                        GameListView.ApplyAsSimpleElement(element, x.Key.GetMasterDataItem().Name, _ =>
                        {
                            var equipmentChangeController = (EquipmentChangeController)context;
                            equipmentChangeController.ChangeEquipment(x.Key);
                            stateMachine.Change(StateSelectEquipmentPartAsync);
                        });
                    });
                });
            var listDocument = CreateList
            (
                listElements,
                0
            );
            inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(StateSelectEquipmentPartAsync);
                })
                .RegisterTo(scope);
            await UniTask.WaitUntilCanceled(scope);
            Object.Destroy(listDocument.gameObject);
        }

        private async UniTask StateSelectArmorHeadAsync(CancellationToken scope)
        {
            var listElements = character.Inventory.Items
                .Where(x => x.Key.ContainsMasterDataArmorHead())
                .Select(x =>
                {
                    return new Action<HKUIDocument>(element =>
                    {
                        GameListView.ApplyAsSimpleElement(element, x.Key.GetMasterDataItem().Name, _ =>
                        {
                            var equipmentChangeController = (EquipmentChangeController)context;
                            equipmentChangeController.ChangeEquipment(x.Key);
                            stateMachine.Change(StateSelectEquipmentPartAsync);
                        });
                    });
                });
            var listDocument = CreateList(listElements, 0);
            inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(StateSelectEquipmentPartAsync);
                })
                .RegisterTo(scope);
            await UniTask.WaitUntilCanceled(scope);
            Object.Destroy(listDocument.gameObject);
        }

        private async UniTask StateSelectArmorBodyAsync(CancellationToken scope)
        {
            var listElements = character.Inventory.Items
                .Where(x => x.Key.ContainsMasterDataArmorBody())
                .Select(x =>
                {
                    return new Action<HKUIDocument>(element =>
                    {
                        GameListView.ApplyAsSimpleElement(element, x.Key.GetMasterDataItem().Name, _ =>
                        {
                            var equipmentChangeController = (EquipmentChangeController)context;
                            equipmentChangeController.ChangeEquipment(x.Key);
                            stateMachine.Change(StateSelectEquipmentPartAsync);
                        });
                    });
                });
            var listDocument = CreateList(listElements, 0);
            inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(StateSelectEquipmentPartAsync);
                })
                .RegisterTo(scope);
            await UniTask.WaitUntilCanceled(scope);
            Object.Destroy(listDocument.gameObject);
        }

        private async UniTask StateSelectArmorArmsAsync(CancellationToken scope)
        {
            var listElements = character.Inventory.Items
                .Where(x => x.Key.ContainsMasterDataArmorArms())
                .Select(x =>
                {
                    return new Action<HKUIDocument>(element =>
                    {
                        GameListView.ApplyAsSimpleElement(element, x.Key.GetMasterDataItem().Name, _ =>
                        {
                            var equipmentChangeController = (EquipmentChangeController)context;
                            equipmentChangeController.ChangeEquipment(x.Key);
                            stateMachine.Change(StateSelectEquipmentPartAsync);
                        });
                    });
                });
            var listDocument = CreateList(listElements, 0);
            inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(StateSelectEquipmentPartAsync);
                })
                .RegisterTo(scope);
            await UniTask.WaitUntilCanceled(scope);
            Object.Destroy(listDocument.gameObject);
        }

        private async UniTask StateSelectArmorLegsAsync(CancellationToken scope)
        {
            var listElements = character.Inventory.Items
                .Where(x => x.Key.ContainsMasterDataArmorLegs())
                .Select(x =>
                {
                    return new Action<HKUIDocument>(element =>
                    {
                        GameListView.ApplyAsSimpleElement(element, x.Key.GetMasterDataItem().Name, _ =>
                        {
                            var equipmentChangeController = (EquipmentChangeController)context;
                            equipmentChangeController.ChangeEquipment(x.Key);
                            stateMachine.Change(StateSelectEquipmentPartAsync);
                        });
                    });
                });
            var listDocument = CreateList(listElements, 0);
            inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(StateSelectEquipmentPartAsync);
                })
                .RegisterTo(scope);
            await UniTask.WaitUntilCanceled(scope);
            Object.Destroy(listDocument.gameObject);
        }

        private async UniTask StateSelectAccessoryAsync(CancellationToken scope)
        {
            var listElements = character.Inventory.Items
                .Where(x => x.Key.ContainsMasterDataAccessory())
                .Select(x =>
                {
                    return new Action<HKUIDocument>(element =>
                    {
                        GameListView.ApplyAsSimpleElement(element, x.Key.GetMasterDataItem().Name, _ =>
                        {
                            var equipmentChangeController = (EquipmentChangeController)context;
                            equipmentChangeController.ChangeEquipment(x.Key);
                            stateMachine.Change(StateSelectEquipmentPartAsync);
                        });
                    });
                });
            var listDocument = CreateList(listElements, 0);
            inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(StateSelectEquipmentPartAsync);
                })
                .RegisterTo(scope);
            await UniTask.WaitUntilCanceled(scope);
            Object.Destroy(listDocument.gameObject);
        }

        private UniTask StateCloseAsync(CancellationToken scope)
        {
            openCompletionSource.TrySetResult();
            return UniTask.CompletedTask;
        }

        private HKUIDocument CreateList
        (
            IEnumerable<Action<HKUIDocument>> elements,
            int initialElement
        )
        {
            return GameListView.Create
            (
                documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.List"),
                elements,
                initialElement
            );
        }

        public interface IContext
        {
        }
    }
}
