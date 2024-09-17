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
                        },
                        _ =>
                        {
                            GameTipsView.SetTip("経験値を消費してレベルアップを行います。");
                        }
                    );
                },
                element =>
                {
                    GameListView.ApplyAsSimpleElement
                    (
                        element,
                        "休憩",
                        _ =>
                        {
                            stateMachine.Change(StateRestAsync);
                        },
                        _ =>
                        {
                            GameTipsView.SetTip("HPとSTを回復します。これは一度しか使用できません。");
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
            CreateHeader("チェックポイント", scope);
            await UniTask.WaitUntilCanceled(scope);
            UnityEngine.Object.Destroy(listDocument.gameObject);
        }

        private async UniTask StateLevelUpAsync(CancellationToken scope)
        {
            var isConfirming = false;
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
                            },
                            character.GrowthParameter.Vitality,
                            "生命力を上げるとHP（ヒットポイント）が増加します。"
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
                                growthParameter.Stamina += x;
                                return growthParameter.Stamina;
                            },
                            character.GrowthParameter.Stamina,
                            "持久力を上げるとST（スタミナ）が増加します。"
                        );
                    },
                    element =>
                    {
                        SetupElement
                        (
                            element,
                            "筋力",
                            x =>
                            {
                                growthParameter.PhysicalStrength += x;
                                return growthParameter.PhysicalStrength;
                            },
                            character.GrowthParameter.PhysicalStrength,
                            "筋力を上げると物理攻撃の威力が増加します。"
                        );
                    },
                    element =>
                    {
                        SetupElement
                        (
                            element,
                            "精神力",
                            x =>
                            {
                                growthParameter.MagicalStrength += x;
                                return growthParameter.MagicalStrength;
                            },
                            character.GrowthParameter.MagicalStrength,
                            "精神力を上げると魔法攻撃の威力が増加します。"
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
                            },
                            character.GrowthParameter.Speed,
                            "素早さを上げると先手を取りやすくなります。"
                        );
                    },
                },
                0
            );
            var informationDocument = UnityEngine.Object.Instantiate(documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Info.LevelUp"));
            var informationViewport = informationDocument.Q<RectTransform>("Viewport");
            var informationElementPrefab = documentBundlePrefab.Q<HKUIDocument>("UIElement.Info");
            var currentLevelInfo = UnityEngine.Object.Instantiate(informationElementPrefab, informationViewport);
            currentLevelInfo.Q<TMP_Text>("Header").text = "レベル";
            Observable.Merge(useExperience)
                .Subscribe(x =>
                {
                    currentLevelInfo.Q<TMP_Text>("Value").text = growthParameter.Level.ToString();
                })
                .RegisterTo(scope);
            var currentExperienceInfo = UnityEngine.Object.Instantiate(informationElementPrefab, informationViewport);
            currentExperienceInfo.Q<TMP_Text>("Header").text = "所持経験値";
            Observable.Merge(useExperience, character.InstanceStatus.ExperienceAsObservable())
                .Subscribe(x =>
                {
                    currentExperienceInfo.Q<TMP_Text>("Value").text = (character.InstanceStatus.ExperienceAsObservable().CurrentValue - useExperience.Value).ToString();
                })
                .RegisterTo(scope);
            var needExperienceInfo = UnityEngine.Object.Instantiate(informationElementPrefab, informationViewport);
            needExperienceInfo.Q<TMP_Text>("Header").text = "必要経験値";
            Observable.Merge(useExperience)
                .Subscribe(x =>
                {
                    needExperienceInfo.Q<TMP_Text>("Value").text = gameRule.ExperienceTable.GetNeedExperience(growthParameter.Level + 1).ToString();
                })
                .RegisterTo(scope);
            inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                .Subscribe(async _ =>
                {
                    if (isConfirming)
                    {
                        return;
                    }
                    if (growthParameter.Level != character.GrowthParameter.Level)
                    {
                        isConfirming = true;
                        var result = await DialogView.ConfirmAsync
                        (
                            documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Dialog"),
                            "レベルアップせずに戻ります。よろしいですか？",
                            new[] { "はい", "いいえ" },
                            0,
                            scope
                        );
                        isConfirming = false;
                        if (result == 0)
                        {
                            stateMachine.Change(StateRootMenuAsync);
                        }
                    }
                    else
                    {
                        stateMachine.Change(StateRootMenuAsync);
                    }
                })
                .RegisterTo(scope);
            CreateHeader("レベルアップ", scope);
            await UniTask.WaitUntilCanceled(scope);
            UnityEngine.Object.Destroy(listDocument.gameObject);
            UnityEngine.Object.Destroy(informationDocument.gameObject);

            void SetupElement
            (
                HKUIDocument element,
                string header,
                Func<int, int> valueSelector,
                int minValue,
                string tip
            )
            {
                GameListView.ApplyAsSimpleElement
                (
                    element,
                    header,
                    async _ =>
                    {
                        if (isConfirming)
                        {
                            return;
                        }
                        if (character.GrowthParameter.Level == growthParameter.Level)
                        {
                            isConfirming = true;
                            await DialogView.ConfirmAsync
                            (
                                documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Dialog"),
                                "パラメーターを割り振ってください",
                                new[] { "OK" },
                                0,
                                scope
                            );
                            isConfirming = false;
                            return;
                        }
                        isConfirming = true;
                        var result = await DialogView.ConfirmAsync
                        (
                            documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Dialog"),
                            "本当にレベルアップしますか？",
                            new[] { "はい", "いいえ" },
                            0,
                            scope
                        );
                        isConfirming = false;
                        if (result == 0)
                        {
                            character.GrowthParameter.Sync(growthParameter, character);
                            character.InstanceStatus.AddExperience(-useExperience.Value);
                            character.Events.OnLevelUp.OnNext(growthParameter);
                            stateMachine.Change(StateRootMenuAsync);
                            AudioManager.PlaySfx("Sfx.Message.22");
                        }
                    },
                    _ =>
                    {
                        GameTipsView.SetTip(tip);
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
                                else if (velocity.x < 0 && CanLevelDown(minValue, valueSelector(0)))
                                {
                                    valueSelector(-1);
                                    growthParameter.Level -= 1;
                                    useExperience.Value -= gameRule.ExperienceTable.GetNeedExperience(growthParameter.Level + 1);
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

        private async UniTask StateRestAsync(CancellationToken scope)
        {
            var dungeonController = TinyServiceLocator.Resolve<DungeonController>();
            if (!dungeonController.CanRestCheckPoint(character.Position))
            {
                await DialogView.ConfirmAsync
                (
                    documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Dialog"),
                    "ここではもう休憩できません。",
                    new[] { "OK" },
                    0,
                    scope
                );
                stateMachine.Change(StateRootMenuAsync);
                return;
            }
            var result = await DialogView.ConfirmAsync
            (
                documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Dialog"),
                "HPとSTを回復しますが一度しか使用できません。\nよろしいですか？",
                new[] { "はい", "いいえ" },
                0,
                scope
            );
            if (result == 0)
            {
                AudioManager.PlaySfx("Sfx.Message.19");
                TinyServiceLocator.Resolve<ScreenEffectView>().Play("Animation.OnRecoveryHitPoint");
                character.InstanceStatus.FullRecovery();
                dungeonController.RestCheckPoint(character.Position);
            }
            stateMachine.Change(StateRootMenuAsync);
        }

        private UniTask StateCloseAsync(CancellationToken scope)
        {
            GameTipsView.SetTip(string.Empty);
            openCompletionSource.TrySetResult();
            return UniTask.CompletedTask;
        }

        private HKUIDocument CreateList
        (
            IEnumerable<Action<HKUIDocument>> elementActivateActions,
            int initialElement
        )
        {
            return GameListView.CreateWithPages
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
            var needExperience = gameRule.ExperienceTable.GetNeedExperience(growthParameter.Level + 1);
            return character.InstanceStatus.ExperienceAsObservable().CurrentValue - useExperience.Value >= needExperience;
        }

        private bool CanLevelDown(int min, int current)
        {
            return character.GrowthParameter.Level < growthParameter.Level && min < current;
        }

        private HKUIDocument CreateHeader(string header, CancellationToken scope)
        {
            return HeaderView.Open(documentBundlePrefab.Q<HKUIDocument>("UI.Game.Header"), header, scope);
        }
    }
}
