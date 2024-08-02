using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using R3.Triggers;
using SoulRPG.CharacterControllers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameSavePointMenuView
    {
        private readonly HKUIDocument documentBundlePrefab;

        private readonly Character character;

        private readonly TinyStateMachine stateMachine;

        private readonly UniTaskCompletionSource openCompletionSource = new();

        private InputController inputController;

        private int vitality;

        private int stamina;

        private int physicalStrength;

        private int magicalStrength;

        private int speed;

        public class ListElement
        {
            public string header;

            public System.Action onClick;

            public System.Action onLeft;

            public System.Action onRight;
        }

        public GameSavePointMenuView(HKUIDocument documentBundlePrefab, Character character)
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
            var listDocument = CreateList(new List<Action<HKUIDocument>>
            {
                element =>
                {
                    GameListView.ApplyAsSimpleElement
                    (
                        element,
                        "レベルアップ",
                        _ =>
                        {
                            stateMachine.Change(StateLevelUpAsync);
                        }
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
            UnityEngine.Object.Destroy(listDocument.gameObject);
        }

        private async UniTask StateLevelUpAsync(CancellationToken scope)
        {
            vitality = character.GrowthParameter.Vitality;
            stamina = character.GrowthParameter.Stamina;
            physicalStrength = character.GrowthParameter.PhysicalStrength;
            magicalStrength = character.GrowthParameter.MagicalStrength;
            speed = character.GrowthParameter.Speed;
            var listDocument = CreateList
            (
                new List<Action<HKUIDocument>>
                {
                    element =>
                    {
                        SetupElement
                        (
                            element,
                            "生命力",
                            x =>
                            {
                                vitality += x;
                                return vitality;
                            }
                        );
                    },
                    element =>
                    {
                        SetupElement
                        (
                            element,
                            "持久力",
                            x =>
                            {
                                stamina += x;
                                return stamina;
                            }
                        );
                    },
                    element =>
                    {
                        SetupElement
                        (
                            element,
                            "物理攻撃力",
                            x =>
                            {
                                physicalStrength += x;
                                return physicalStrength;
                            }
                        );
                    },
                    element =>
                    {
                        SetupElement
                        (
                            element,
                            "魔法攻撃力",
                            x =>
                            {
                                magicalStrength += x;
                                return magicalStrength;
                            }
                        );
                    },
                    element =>
                    {
                        SetupElement
                        (
                            element,
                            "素早さ",
                            x =>
                            {
                                speed += x;
                                return speed;
                            }
                        );
                    },
                },
                0
            );
            inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(StateRootMenuAsync);
                })
                .RegisterTo(scope);
            await UniTask.WaitUntilCanceled(scope);
            UnityEngine.Object.Destroy(listDocument.gameObject);

            void SetupElement
            (
                HKUIDocument element,
                string header,
                Func<int, int> valueSelector
            )
            {
                GameListView.ApplyAsSimpleElement
                (
                    element,
                    header,
                    _ =>
                    {
                    }
                );
                var horizontalInterface = CreateHorizontalInterface(element);
                UpdateHorizontalInterfaceMessage(horizontalInterface, valueSelector(0).ToString("00"));
                var button = element.Q<Button>("Button");
                button.OnSelectAsObservable()
                    .Subscribe(_ =>
                    {
                        inputController.InputActions.UI.Navigate.OnPerformedAsObservable()
                            .TakeUntil(button.OnDeselectAsObservable())
                            .Subscribe(x =>
                            {
                                var velocity = x.ReadValue<Vector2>();
                                if (velocity.x == 0)
                                {
                                    return;
                                }
                                if (velocity.x > 0)
                                {
                                    valueSelector(1);
                                }
                                else
                                {
                                    valueSelector(-1);
                                }
                                UpdateHorizontalInterfaceMessage(horizontalInterface, valueSelector(0).ToString("00"));
                            })
                            .RegisterTo(scope);
                    })
                    .RegisterTo(scope);
            }
        }

        private UniTask StateCloseAsync(CancellationToken scope)
        {
            openCompletionSource.TrySetResult();
            return UniTask.CompletedTask;
        }

        private HKUIDocument CreateList
        (
            IEnumerable<System.Action<HKUIDocument>> elementActivateActions,
            int initialElement
        )
        {
            return GameListView.Create
            (
                documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.List"),
                elementActivateActions,
                initialElement
            );
        }

        private HKUIDocument CreateHorizontalInterface(HKUIDocument listElement)
        {
            var parent = listElement.Q<RectTransform>("RightPrompt");
            var document = UnityEngine.Object.Instantiate(documentBundlePrefab.Q<HKUIDocument>("UIElement.ListElement.HorizontalInterface"), parent);
            return document;
        }

        private static void UpdateHorizontalInterfaceMessage(HKUIDocument horizontalInterface, string message)
        {
            horizontalInterface.Q<TMP_Text>("Message").text = message;
        }
    }
}
