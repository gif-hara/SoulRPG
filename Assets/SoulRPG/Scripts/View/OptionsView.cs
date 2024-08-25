using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class OptionsView
    {
        public static UniTask OpenAsync(HKUIDocument documentPrefab, CancellationToken scope)
        {
            var document = UnityEngine.Object.Instantiate(documentPrefab);
            var source = new UniTaskCompletionSource();
            var stateMachine = new TinyStateMachine();
            HKUIDocument currentTabObject = null;
            HKUIDocument currentContentsObject = null;
            var inputController = TinyServiceLocator.Resolve<InputController>();
            var tabAreaDocument = document.Q<HKUIDocument>("Area.Tab");
            var contentsAreaDocument = document.Q<HKUIDocument>("Area.Contents");
            var categoryNames = new List<string>
            {
                "Sound",
                "GameSettings",
            };
            foreach (var categoryName in categoryNames)
            {
                SetActiveTab(tabAreaDocument.Q<HKUIDocument>(categoryName));
                SetAcitveContents(contentsAreaDocument.Q<HKUIDocument>(categoryName));
            }
            var categoryList = new List<Func<CancellationToken, UniTask>>
            {
                StateSoundAsync,
                StateGameSettingsAsync,
            };
            var currentCategoryIndex = 0;
            inputController.PushInputType(InputController.InputType.Options, scope);
            inputController.InputActions.Options.Cancel.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    source.TrySetResult();
                })
                .AddTo(scope);
            inputController.InputActions.Options.NextTab.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    currentCategoryIndex++;
                    if (currentCategoryIndex >= categoryList.Count)
                    {
                        currentCategoryIndex = 0;
                    }
                    stateMachine.Change(categoryList[currentCategoryIndex]);
                })
                .RegisterTo(scope);
            inputController.InputActions.Options.PreviousTab.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    currentCategoryIndex--;
                    if (currentCategoryIndex < 0)
                    {
                        currentCategoryIndex = categoryList.Count - 1;
                    }
                    stateMachine.Change(categoryList[currentCategoryIndex]);
                })
                .RegisterTo(scope);
            scope.Register(() =>
                {
                    stateMachine.Dispose();
                    if (document == null)
                    {
                        return;
                    }
                    UnityEngine.Object.Destroy(document.gameObject);
                });
            stateMachine.Change(categoryList[currentCategoryIndex]);
            return source.Task;

            void SetActiveTab(HKUIDocument tab)
            {
                if (currentTabObject != null)
                {
                    currentTabObject.Q<CanvasGroup>("LeftArrow").alpha = 0;
                    currentTabObject.Q<CanvasGroup>("RightArrow").alpha = 0;
                }
                currentTabObject = tab;
                currentTabObject.Q<CanvasGroup>("LeftArrow").alpha = 1;
                currentTabObject.Q<CanvasGroup>("RightArrow").alpha = 1;
            }

            void SetAcitveContents(HKUIDocument contents)
            {
                if (currentContentsObject != null)
                {
                    currentContentsObject.gameObject.SetActive(false);
                }
                currentContentsObject = contents;
                currentContentsObject.gameObject.SetActive(true);
            }

            UniTask StateSoundAsync(CancellationToken scope)
            {
                var contents = contentsAreaDocument.Q<HKUIDocument>("Sound");
                HKUIDocument currentVolume = null;
                var volumeNames = new List<string>
                {
                    "Master",
                    "Bgm",
                    "Sfx",
                };
                var volumeExposeNames = new List<string>
                {
                    "MasterVolume",
                    "BgmVolume",
                    "SfxVolume",
                };
                var tips = new List<string>
                {
                    "ゲーム全体の音量を設定します。",
                    "BGMの音量を設定します。",
                    "効果音の音量を設定します。",
                };
                var currentVolumeIndex = 0;
                for (var i = 0; i < volumeNames.Count; i++)
                {
                    var volume = contents.Q<HKUIDocument>(volumeNames[i]);
                    SetActiveVolume(volume);
                    SetSliderValue(volume, AudioManager.GetVolume(volumeExposeNames[i]));
                }
                SetActiveTab(tabAreaDocument.Q<HKUIDocument>("Sound"));
                SetAcitveContents(contents);
                SetActiveVolume(contents.Q<HKUIDocument>(volumeNames[currentVolumeIndex]));
                GameTipsView.SetTip(tips[currentVolumeIndex]);
                inputController.InputActions.Options.Navigate.OnPerformedAsObservable()
                    .Subscribe(context =>
                    {
                        var value = context.ReadValue<Vector2>();
                        if (value.y > 0)
                        {
                            currentVolumeIndex--;
                            if (currentVolumeIndex < 0)
                            {
                                currentVolumeIndex = volumeNames.Count - 1;
                            }
                            SetActiveVolume(contents.Q<HKUIDocument>(volumeNames[currentVolumeIndex]));
                            GameTipsView.SetTip(tips[currentVolumeIndex]);
                            AudioManager.PlaySFX("Sfx.Select.0");
                        }
                        else if (value.y < 0)
                        {
                            currentVolumeIndex++;
                            if (currentVolumeIndex >= volumeNames.Count)
                            {
                                currentVolumeIndex = 0;
                            }
                            SetActiveVolume(contents.Q<HKUIDocument>(volumeNames[currentVolumeIndex]));
                            GameTipsView.SetTip(tips[currentVolumeIndex]);
                            AudioManager.PlaySFX("Sfx.Select.0");
                        }
                        if (value.x > 0)
                        {
                            var volume = AudioManager.GetVolume(volumeExposeNames[currentVolumeIndex]);
                            volume += 0.1f;
                            AudioManager.SetVolume(volumeExposeNames[currentVolumeIndex], volume);
                            SetSliderValue(contents.Q<HKUIDocument>(volumeNames[currentVolumeIndex]), volume);
                            AudioManager.PlaySFX("Sfx.Message.0");
                        }
                        else if (value.x < 0)
                        {
                            var volume = AudioManager.GetVolume(volumeExposeNames[currentVolumeIndex]);
                            volume -= 0.1f;
                            AudioManager.SetVolume(volumeExposeNames[currentVolumeIndex], volume);
                            SetSliderValue(contents.Q<HKUIDocument>(volumeNames[currentVolumeIndex]), volume);
                            AudioManager.PlaySFX("Sfx.Message.0");
                        }
                    })
                    .RegisterTo(scope);

                return UniTask.CompletedTask;
                void SetActiveVolume(HKUIDocument volume)
                {
                    if (currentVolume != null)
                    {
                        currentVolume.Q<HKUIDocument>("Slider").Q<CanvasGroup>("LeftArrow").alpha = 0;
                        currentVolume.Q<HKUIDocument>("Slider").Q<CanvasGroup>("RightArrow").alpha = 0;
                    }
                    currentVolume = volume;
                    currentVolume.Q<HKUIDocument>("Slider").Q<CanvasGroup>("LeftArrow").alpha = 1;
                    currentVolume.Q<HKUIDocument>("Slider").Q<CanvasGroup>("RightArrow").alpha = 1;
                }
                void SetSliderValue(HKUIDocument volume, float value)
                {
                    volume.Q<HKUIDocument>("Slider").Q<Slider>("Slider").value = value;
                }
            }

            UniTask StateGameSettingsAsync(CancellationToken scope)
            {
                SetActiveTab(tabAreaDocument.Q<HKUIDocument>("GameSettings"));
                SetAcitveContents(contentsAreaDocument.Q<HKUIDocument>("GameSettings"));
                return UniTask.CompletedTask;
            }
        }
    }
}
