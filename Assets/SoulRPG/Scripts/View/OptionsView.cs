using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class OptionsView
    {
        public static UniTask OpenAsync(HKUIDocument documentBundlePrefab, CancellationToken scope)
        {
            var document = UnityEngine.Object.Instantiate(documentBundlePrefab.Q<HKUIDocument>("UI.Options"));
            var header = HeaderView.Open(documentBundlePrefab.Q<HKUIDocument>("UI.Game.Header"), "システム".Localized(), scope);
            var source = new UniTaskCompletionSource();
            var stateMachine = new TinyStateMachine();
            HKUIDocument currentTabObject = null;
            HKUIDocument currentContentsObject = null;
            var inputController = TinyServiceLocator.Resolve<InputController>();
            var tabAreaDocument = document.Q<HKUIDocument>("Area.Tab");
            var contentsAreaDocument = document.Q<HKUIDocument>("Area.Contents");
            var categoryNames = new List<string>
            {
                "Screen",
                "Sound",
                "GameSettings",
                "Language",
            };
            var categoryList = new List<Func<CancellationToken, UniTask>>
            {
                StateScreenAsync,
                StateSoundAsync,
                StateGameSettingsAsync,
                StateLanguageAsync,
            };
            foreach (var categoryName in categoryNames)
            {
                var tab = tabAreaDocument.Q<HKUIDocument>(categoryName);
                SetActiveTab(tab);
                SetAcitveContents(contentsAreaDocument.Q<HKUIDocument>(categoryName));
                tab.Q<Button>("Button").OnClickAsObservable()
                .Subscribe(_ =>
                {
                    stateMachine.Change(categoryList[categoryNames.IndexOf(categoryName)]);
                })
                .RegisterTo(scope);
            }
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
                AudioManager.PlaySfx("Sfx.Message.0");
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
                    "ゲーム全体の音量を設定する。".Localized(),
                    "BGMの音量を設定する。".Localized(),
                    "効果音の音量を設定する。".Localized(),
                };
                var initialActions = new List<Action<HKUIDocument>>
                {
                    x =>
                    {
                        x.Q<HKUIDocument>("Slider").Q<Slider>("Slider").OnValueChangedAsObservable()
                            .Skip(1)
                            .Subscribe(value =>
                            {
                                var saveData = SaveData.LoadSafe();
                                saveData.audioData.masterVolume = value;
                                saveData.Save();
                                AudioManager.SetVolume("MasterVolume", value);
                            })
                            .RegisterTo(scope);
                    },
                    x =>
                    {
                        x.Q<HKUIDocument>("Slider").Q<Slider>("Slider").OnValueChangedAsObservable()
                            .Skip(1)
                            .Subscribe(value =>
                            {
                                var saveData = SaveData.LoadSafe();
                                saveData.audioData.bgmVolume = value;
                                saveData.Save();
                                AudioManager.SetVolume("BgmVolume", value);
                            })
                            .RegisterTo(scope);
                    },
                    x =>
                    {
                        x.Q<HKUIDocument>("Slider").Q<Slider>("Slider").OnValueChangedAsObservable()
                            .Skip(1)
                            .Subscribe(value =>
                            {
                                var saveData = SaveData.LoadSafe();
                                saveData.audioData.sfxVolume = value;
                                saveData.Save();
                                AudioManager.SetVolume("SfxVolume", value);
                            })
                            .RegisterTo(scope);
                    },
                };
                var currentVolumeIndex = 0;
                for (var i = 0; i < volumeNames.Count; i++)
                {
                    var volume = contents.Q<HKUIDocument>(volumeNames[i]);
                    initialActions[i](contents.Q<HKUIDocument>(volumeNames[i]));
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
                            AudioManager.PlaySfx("Sfx.Select.0");
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
                            AudioManager.PlaySfx("Sfx.Select.0");
                        }
                        if (value.x > 0)
                        {
                            var volume = AudioManager.GetVolume(volumeExposeNames[currentVolumeIndex]);
                            volume += 0.1f;
                            SetSliderValue(contents.Q<HKUIDocument>(volumeNames[currentVolumeIndex]), volume);
                            AudioManager.PlaySfx("Sfx.Message.0");
                        }
                        else if (value.x < 0)
                        {
                            var volume = AudioManager.GetVolume(volumeExposeNames[currentVolumeIndex]);
                            volume -= 0.1f;
                            SetSliderValue(contents.Q<HKUIDocument>(volumeNames[currentVolumeIndex]), volume);
                            AudioManager.PlaySfx("Sfx.Message.0");
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
                    EventSystem.current.SetSelectedGameObject(currentVolume.Q<Button>("Button").gameObject);
                }
                void SetSliderValue(HKUIDocument volume, float value)
                {
                    volume.Q<HKUIDocument>("Slider").Q<Slider>("Slider").value = value;
                }
            }

            UniTask StateGameSettingsAsync(CancellationToken scope)
            {
                AudioManager.PlaySfx("Sfx.Message.0");
                var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
                var areaDocument = contentsAreaDocument.Q<HKUIDocument>("GameSettings");
                var elementIndex = 0;
                var elements = new List<HKUIDocument>
                {
                    areaDocument.Q<HKUIDocument>("IsRotationMiniMap"),
                };
                var tips = new List<string>
                {
                    "ミニマップの回転をプレイヤーの向きと同期を取るか設定する。".Localized(),
                };
                var onSubmitActions = new List<Action>
                {
                    () =>
                    {
                        AudioManager.PlaySfx("Sfx.Message.0");
                        var saveData = SaveData.LoadSafe();
                        saveData.gameSettingData.isRotationMiniMap = !saveData.gameSettingData.isRotationMiniMap;
                        saveData.Save();
                        gameEvents.OnChangeIsRotationMiniMap.OnNext(saveData.gameSettingData.isRotationMiniMap);
                        var element = elements[elementIndex];
                        element.Q<HKUIDocument>("Message")
                            .Q<TMP_Text>("Message")
                            .text = saveData.gameSettingData.isRotationMiniMap ? "オン".Localized() : "オフ".Localized();
                    },
                };
                var onInitializeActions = new List<Action<HKUIDocument>>
                {
                    element =>
                    {
                        var saveData = SaveData.LoadSafe();
                        element.Q<HKUIDocument>("Message")
                            .Q<TMP_Text>("Message")
                            .text = saveData.gameSettingData.isRotationMiniMap ? "オン".Localized() : "オフ".Localized();
                    },
                };
                var onNavigateActions = new List<Action<bool, HKUIDocument>>
                {
                    (isLeft, element) =>
                    {
                        onSubmitActions[elementIndex]();
                    }
                };
                inputController.InputActions.Options.Navigate.OnPerformedAsObservable()
                    .Subscribe(context =>
                    {
                        var value = context.ReadValue<Vector2>();
                        if (value.y > 0)
                        {
                            elementIndex--;
                            if (elementIndex < 0)
                            {
                                elementIndex = elements.Count - 1;
                            }
                            SetActive();
                        }
                        else if (value.y < 0)
                        {
                            elementIndex++;
                            if (elementIndex >= elements.Count)
                            {
                                elementIndex = 0;
                            }
                            SetActive();
                        }
                        else if (value.x > 0)
                        {
                            onNavigateActions[elementIndex](true, elements[elementIndex]);
                        }
                        else if (value.x < 0)
                        {
                            onNavigateActions[elementIndex](false, elements[elementIndex]);
                        }
                    })
                    .RegisterTo(scope);
                inputController.InputActions.Options.Submit.OnPerformedAsObservable()
                    .Subscribe(_ =>
                    {
                        onSubmitActions[elementIndex]();
                    })
                    .RegisterTo(scope);

                for (var i = 0; i < elements.Count; i++)
                {
                    onInitializeActions[i](elements[i]);
                }
                SetActiveTab(tabAreaDocument.Q<HKUIDocument>("GameSettings"));
                SetAcitveContents(areaDocument);
                SetActive();
                return UniTask.CompletedTask;
                void SetActive()
                {
                    GameTipsView.SetTip(tips[elementIndex]);
                    AudioManager.PlaySfx("Sfx.Select.0");
                    EventSystem.current.SetSelectedGameObject(elements[elementIndex].Q<Button>("Button").gameObject);
                }
            }

            UniTask StateLanguageAsync(CancellationToken scope)
            {
                AudioManager.PlaySfx("Sfx.Message.0");
                string[] languageKeys = { "ja-JP", "en", "zh", "zh-TW", "fr-FR", "de", "es" };
                string[] languageNames = { "日本語", "English", "中文", "中文(繁体)", "Français", "Deutsch", "Español" };
                var saveData = SaveData.LoadSafe();
                var areaDocument = contentsAreaDocument.Q<HKUIDocument>("Language");
                var languageIndex = Array.IndexOf(languageKeys, saveData.languageData.language);
                if (languageIndex == -1)
                {
                    languageIndex = 0;
                }
                var elements = new List<HKUIDocument>
                {
                    areaDocument.Q<HKUIDocument>("Language"),
                };
                var tips = new List<string>
                {
                    "日本語に設定する。",
                    "英語に設定する。",
                    "中国語に設定する。",
                    "繁体字に設定する。",
                    "フランス語に設定する。",
                    "ドイツ語に設定する。",
                    "スペイン語に設定する。",
                };
                var onInitializeActions = new List<Action<HKUIDocument>>
                {
                    element =>
                    {
                        element.Q<HKUIDocument>("Message")
                            .Q<TMP_Text>("Message")
                            .text = languageNames[languageIndex];
                    },
                };
                var onHorizontalActions = new List<Action<bool, HKUIDocument>>
                {
                    (isLeft, element) =>
                    {
                        AudioManager.PlaySfx("Sfx.Message.0");
                        languageIndex += isLeft ? -1 : 1;
                        if (languageIndex < 0)
                        {
                            languageIndex = languageKeys.Length - 1;
                        }
                        else if (languageIndex >= languageKeys.Length)
                        {
                            languageIndex = 0;
                        }
                        saveData.languageData.language = languageKeys[languageIndex];
                        LocalizationSettings.SelectedLocale = Locale.CreateLocale(languageKeys[languageIndex]);
                        element.Q<HKUIDocument>("Message")
                            .Q<TMP_Text>("Message")
                            .text = languageNames[languageIndex];
                        GameTipsView.SetTip(tips[languageIndex].Localized());
                        saveData.Save();
                    }
                };
                inputController.InputActions.Options.Navigate.OnPerformedAsObservable()
                    .Subscribe(context =>
                    {
                        var value = context.ReadValue<Vector2>();
                        if (value.x > 0)
                        {
                            onHorizontalActions[0](true, elements[0]);
                        }
                        else if (value.x < 0)
                        {
                            onHorizontalActions[0](false, elements[0]);
                        }
                    })
                    .RegisterTo(scope);
                SetActiveTab(tabAreaDocument.Q<HKUIDocument>("Language"));
                SetAcitveContents(areaDocument);
                for (var i = 0; i < elements.Count; i++)
                {
                    onInitializeActions[i](elements[i]);
                }
                GameTipsView.SetTip(tips[languageIndex].Localized());
                elements[0].Q<Button>("Button").Select();
                return UniTask.CompletedTask;
            }

            UniTask StateScreenAsync(CancellationToken scope)
            {
                AudioManager.PlaySfx("Sfx.Message.0");
                SetActiveTab(tabAreaDocument.Q<HKUIDocument>("Screen"));
                SetAcitveContents(contentsAreaDocument.Q<HKUIDocument>("Screen"));
                foreach (var i in Screen.resolutions)
                {
                    Debug.Log(i);
                }
                return UniTask.CompletedTask;
            }
        }
    }
}
