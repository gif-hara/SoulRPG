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

        private readonly InputController inputController;

        private CharacterGrowthParameter growthParameter;

        private ReactiveProperty<int> useExperience = new(0);

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
            var gameRule = TinyServiceLocator.Resolve<GameRule>();
            growthParameter = new CharacterGrowthParameter(character.GrowthParameter);
            useExperience = new ReactiveProperty<int>(0);
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
                                growthParameter.Vitality += x;
                                return growthParameter.Vitality;
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
                                growthParameter.stamina += x;
                                return growthParameter.Stamina;
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
                                growthParameter.PhysicalStrength += x;
                                return growthParameter.PhysicalStrength;
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
                                growthParameter.MagicalStrength += x;
                                return growthParameter.MagicalStrength;
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
                                growthParameter.Speed += x;
                                return growthParameter.Speed;
                            }
                        );
                    },
                },
                0
            );
            var userData = TinyServiceLocator.Resolve<UserData>();
            var informationDocument = UnityEngine.Object.Instantiate(documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Info.LevelUp"));
            var informationViewport = informationDocument.Q<RectTransform>("Viewport");
            var informationElementPrefab = documentBundlePrefab.Q<HKUIDocument>("UIElement.Info");
            var informationElement = UnityEngine.Object.Instantiate(informationElementPrefab, informationViewport);
            informationElement.Q<TMP_Text>("Header").text = "所持経験値";
            Observable.Merge(useExperience, userData.Experience)
                .Subscribe(x =>
                {
                    informationElement.Q<TMP_Text>("Value").text = (userData.Experience.CurrentValue - useExperience.Value).ToString();
                })
                .RegisterTo(scope);
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
                var leftArrow = horizontalInterface.Q<CanvasGroup>("LeftArrow");
                leftArrow.alpha = 0;
                var rightArrow = horizontalInterface.Q<CanvasGroup>("RightArrow");
                rightArrow.alpha = 0;
                var button = element.Q<Button>("Button");
                button.OnSelectAsObservable()
                    .Subscribe(_ =>
                    {
                        leftArrow.alpha = 1;
                        rightArrow.alpha = 1;
                        inputController.InputActions.UI.Navigate.OnPerformedAsObservable()
                            .TakeUntil(button.OnDeselectAsObservable())
                            .Subscribe(x =>
                            {
                                var velocity = x.ReadValue<Vector2>();
                                if (velocity.x == 0)
                                {
                                    return;
                                }
                                if (velocity.x > 0 && CanLevelUp())
                                {
                                    valueSelector(1);
                                    growthParameter.Level += 1;
                                    useExperience.Value += gameRule.ExperienceTable.GetNeedExperience(growthParameter.Level);
                                }
                                else if (velocity.x < 0 && CanLevelDown())
                                {
                                    valueSelector(-1);
                                    useExperience.Value -= gameRule.ExperienceTable.GetNeedExperience(growthParameter.Level);
                                    growthParameter.Level -= 1;
                                }
                                UpdateHorizontalInterfaceMessage(horizontalInterface, valueSelector(0).ToString("00"));
                            })
                            .RegisterTo(scope);
                    })
                    .RegisterTo(scope);
                button.OnDeselectAsObservable()
                    .Subscribe(_ =>
                    {
                        leftArrow.alpha = 0;
                        rightArrow.alpha = 0;
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
            IEnumerable<Action<HKUIDocument>> elementActivateActions,
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

        private bool CanLevelUp()
        {
            var gameRule = TinyServiceLocator.Resolve<GameRule>();
            var userData = TinyServiceLocator.Resolve<UserData>();
            var needExperience = gameRule.ExperienceTable.GetNeedExperience(growthParameter.Level + 1);
            return userData.Experience.CurrentValue - useExperience.Value >= needExperience;
        }

        private bool CanLevelDown()
        {
            return character.GrowthParameter.Level < growthParameter.Level;
        }
    }
}
