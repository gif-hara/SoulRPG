using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.CharacterControllers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameMenuView
    {
        private readonly HKUIDocument documentBundlePrefab;

        private readonly Character character;

        private readonly TinyStateMachine stateMachine;

        private readonly UniTaskCompletionSource openCompletionSource = new();

        private InputController inputController;

        private IContext context;

        public class ListElement
        {
            public string header;

            public System.Action onClick;

            public System.Action onLeft;

            public System.Action onRight;
        }

        public GameMenuView(HKUIDocument documentBundlePrefab, Character character)
        {
            this.documentBundlePrefab = documentBundlePrefab;
            this.character = character;
            this.stateMachine = new TinyStateMachine();
            inputController = TinyServiceLocator.Resolve<InputController>();
        }

        public async UniTask OpenAsync()
        {
            var inputController = TinyServiceLocator.Resolve<InputController>();
            inputController.ChangeInputType(InputController.InputType.UI);
            stateMachine.Change(StateRootMenuAsync);
            await openCompletionSource.Task;
            inputController.ChangeInputType(InputController.InputType.InGame);
        }

        private async UniTask StateRootMenuAsync(CancellationToken scope)
        {
            var actions = TinyServiceLocator.Resolve<InputController>().InputActions.UI;
            var listDocument = CreateListDocument(new List<ListElement>
            {
                new()
                {
                    header = "装備",
                    onClick = () => stateMachine.Change(StateSelectEquipmentPartAsync)
                },
                new()
                {
                    header = "道具",
                    onClick = () =>
                    {
                        Debug.Log("道具");
                    },
                    onLeft = () =>
                    {
                        Debug.Log("左");
                    },
                    onRight = () =>
                    {
                        Debug.Log("右");
                    }
                },
                new()
                {
                    header = "ステータス",
                    onClick = () =>
                    {
                        Debug.Log("ステータス");
                    }
                },
                new()
                {
                    header = "システム",
                    onClick = () =>
                    {
                        Debug.Log("システム");
                    }
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
                return new ListElement()
                {
                    header = $"武器{i + 1}: {weaponName}",
                    onClick = () =>
                    {
                        context = new EquipmentChangeController(character, (EquipmentChangeController.PartType)i + (int)EquipmentChangeController.PartType.Weapon1);
                        stateMachine.Change(StateSelectWeaponAsync);
                    }
                };
            });
            var headElement = new ListElement()
            {
                header = $"頭: {(character.Equipment.HeadId == 0 ? "なし" : character.Equipment.HeadId.GetMasterDataItem().Name)}",
                onClick = () =>
                {
                    context = new EquipmentChangeController(character, EquipmentChangeController.PartType.Head);
                    stateMachine.Change(StateSelectArmorHeadAsync);
                }
            };
            var bodyElement = new ListElement()
            {
                header = $"胴: {(character.Equipment.BodyId == 0 ? "なし" : character.Equipment.BodyId.GetMasterDataItem().Name)}",
                onClick = () =>
                {
                    context = new EquipmentChangeController(character, EquipmentChangeController.PartType.Body);
                    stateMachine.Change(StateSelectArmorBodyAsync);
                }
            };
            var armElement = new ListElement()
            {
                header = $"腕: {(character.Equipment.ArmId == 0 ? "なし" : character.Equipment.ArmId.GetMasterDataItem().Name)}",
                onClick = () =>
                {
                    context = new EquipmentChangeController(character, EquipmentChangeController.PartType.Arm);
                    stateMachine.Change(StateSelectArmorArmsAsync);
                }
            };
            var legElement = new ListElement()
            {
                header = $"脚: {(character.Equipment.LegId == 0 ? "なし" : character.Equipment.LegId.GetMasterDataItem().Name)}",
                onClick = () =>
                {
                    context = new EquipmentChangeController(character, EquipmentChangeController.PartType.Leg);
                    stateMachine.Change(StateSelectArmorLegsAsync);
                }
            };
            var accessoryElements = character.Equipment.GetAccessoryIds().Select((x, i) =>
            {
                var accessoryName = x == 0 ? "なし" : x.GetMasterDataItem().Name;
                return new ListElement()
                {
                    header = $"アクセサリ{i + 1}: {accessoryName}",
                    onClick = () =>
                    {
                        context = new EquipmentChangeController(character, (EquipmentChangeController.PartType)i + (int)EquipmentChangeController.PartType.Accessory1);
                        stateMachine.Change(StateSelectAccessoryAsync);
                    }
                };
            });
            var listElements = new List<ListElement>();
            listElements.AddRange(weaponElements);
            listElements.Add(headElement);
            listElements.Add(bodyElement);
            listElements.Add(armElement);
            listElements.Add(legElement);
            listElements.AddRange(accessoryElements);

            var listDocument = CreateListDocument(listElements, 0);
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
                    var itemName = x.Key.GetMasterDataItem().Name;
                    return new ListElement()
                    {
                        header = itemName,
                        onClick = () =>
                        {
                            var equipmentChangeController = (EquipmentChangeController)context;
                            equipmentChangeController.ChangeEquipment(x.Key);
                            stateMachine.Change(StateSelectEquipmentPartAsync);
                        }
                    };
                });
            var listDocument = CreateListDocument(listElements, 0);
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
                    var itemName = x.Key.GetMasterDataItem().Name;
                    return new ListElement
                    {
                        header = itemName,
                        onClick = () =>
                        {
                            var equipmentChangeController = (EquipmentChangeController)context;
                            equipmentChangeController.ChangeEquipment(x.Key);
                            stateMachine.Change(StateSelectEquipmentPartAsync);
                        }
                    };
                });
            var listDocument = CreateListDocument(listElements, 0);
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
                    var itemName = x.Key.GetMasterDataItem().Name;
                    return new ListElement
                    {
                        header = itemName,
                        onClick = () =>
                        {
                            var equipmentChangeController = (EquipmentChangeController)context;
                            equipmentChangeController.ChangeEquipment(x.Key);
                            stateMachine.Change(StateSelectEquipmentPartAsync);
                        }
                    };
                });
            var listDocument = CreateListDocument(listElements, 0);
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
                    var itemName = x.Key.GetMasterDataItem().Name;
                    return new ListElement
                    {
                        header = itemName,
                        onClick = () =>
                        {
                            var equipmentChangeController = (EquipmentChangeController)context;
                            equipmentChangeController.ChangeEquipment(x.Key);
                            stateMachine.Change(StateSelectEquipmentPartAsync);
                        }
                    };
                });
            var listDocument = CreateListDocument(listElements, 0);
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
                    var itemName = x.Key.GetMasterDataItem().Name;
                    return new ListElement
                    {
                        header = itemName,
                        onClick = () =>
                        {
                            var equipmentChangeController = (EquipmentChangeController)context;
                            equipmentChangeController.ChangeEquipment(x.Key);
                            stateMachine.Change(StateSelectEquipmentPartAsync);
                        }
                    };
                });
            var listDocument = CreateListDocument(listElements, 0);
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
                    var itemName = x.Key.GetMasterDataItem().Name;
                    return new ListElement
                    {
                        header = itemName,
                        onClick = () =>
                        {
                            var equipmentChangeController = (EquipmentChangeController)context;
                            equipmentChangeController.ChangeEquipment(x.Key);
                            stateMachine.Change(StateSelectEquipmentPartAsync);
                        }
                    };
                });
            var listDocument = CreateListDocument(listElements, 0);
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

        private HKUIDocument CreateListDocument(
            IEnumerable<ListElement> listElements,
            int initialElement
            )
        {
            var document = Object.Instantiate(documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.List"));
            var listParent = document.Q<RectTransform>("Area.List");
            var listElementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            var index = 0;
            foreach (var listElement in listElements)
            {
                var element = Object.Instantiate(listElementPrefab, listParent);
                var buttonObject = element.Q("Button");
                element.Q<TMP_Text>("Header").text = listElement.header;
                var button = element.Q<Button>("Button");
                button.OnClickAsObservable()
                    .Subscribe(_ =>
                    {
                        listElement.onClick();
                    })
                    .RegisterTo(element.destroyCancellationToken);
                var navigation = button.navigation;
                navigation.mode = Navigation.Mode.Vertical;
                navigation.wrapAround = true;
                button.navigation = navigation;
                inputController.InputActions.UI.Navigate.OnPerformedAsObservable()
                    .Where(x => x.ReadValue<Vector2>().x != 0)
                    .Where(_ => EventSystem.current.currentSelectedGameObject == buttonObject)
                    .Subscribe(x =>
                    {
                        if (x.ReadValue<Vector2>().x < 0)
                        {
                            listElement.onLeft?.Invoke();
                        }
                        else if (x.ReadValue<Vector2>().x > 0)
                        {
                            listElement.onRight?.Invoke();
                        }
                    })
                    .RegisterTo(element.destroyCancellationToken);
                if (index == initialElement)
                {
                    EventSystem.current.SetSelectedGameObject(button.gameObject);
                }
                index++;
            }
            return document;
        }

        public interface IContext
        {
        }
    }
}
