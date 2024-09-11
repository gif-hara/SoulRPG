using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using HK.Framework;
using R3;
using SoulRPG.CharacterControllers;
using Unity.Mathematics;
using UnityEngine;

namespace SoulRPG
{
#if DEBUG
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameDebugWindowView
    {
        public static async UniTask OpenAsync(HKUIDocument documentBundlePrefab, CancellationToken scope)
        {
            var inputController = TinyServiceLocator.Resolve<InputController>();
            var stateMachine = new TinyStateMachine();
            var source = new UniTaskCompletionSource();
            var player = TinyServiceLocator.Resolve<Character>("Player");
            var dungeonController = TinyServiceLocator.Resolve<DungeonController>();
            var battleDebugData = TinyServiceLocator.Resolve<BattleDebugData>();
            inputController.PushInputType(InputController.InputType.UI);
            stateMachine.Change(StateRootAsync);
            await source.Task;
            stateMachine.Dispose();
            inputController.PopInputType();

            async UniTask StateRootAsync(CancellationToken scope)
            {
                inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                    .Subscribe(_ =>
                    {
                        AudioManager.PlaySfx("Sfx.Cancel.0");
                        source.TrySetResult();
                    })
                    .RegisterTo(scope);
                var listDocument = CreateList(documentBundlePrefab, new List<Action<HKUIDocument>>
                    {
                        element =>
                        {
                            GameListView.ApplyAsSimpleElement
                            (
                                element,
                                "全アイテム追加",
                                _ =>
                                {
                                    AudioManager.PlaySfx("Sfx.Message.0");
                                    foreach (var i in TinyServiceLocator.Resolve<MasterData>().Items.List)
                                    {
                                        player.Inventory.Add(i.Id, 1);
                                    }
                                    ConfirmOkOnlyAsync(documentBundlePrefab, "全アイテムを追加しました", scope).Forget();
                                }
                            );
                        },
                        element =>
                        {
                            GameListView.ApplyAsSimpleElement
                            (
                                element,
                                "バトル",
                                _ =>
                                {
                                    AudioManager.PlaySfx("Sfx.Message.0");
                                    stateMachine.Change(StateSelectBattleAsync);
                                }
                            );
                        },
                        element =>
                        {
                            GameListView.ApplyAsSimpleElement
                            (
                                element,
                                "経験値追加",
                                _ =>
                                {
                                    AudioManager.PlaySfx("Sfx.Message.0");
                                    player.InstanceStatus.AddExperience(TinyServiceLocator.Resolve<GameRule>().DebugAddExperience);
                                    ConfirmOkOnlyAsync(documentBundlePrefab, $"経験値を{TinyServiceLocator.Resolve<GameRule>().DebugAddExperience}追加しました", scope).Forget();
                                }
                            );
                        },
                        element =>
                        {
                            GameListView.ApplyAsSimpleElement
                            (
                                element,
                                "視界良好",
                                _ =>
                                {
                                    AudioManager.PlaySfx("Sfx.Message.0");
                                    dungeonController.DebugAddAllReachedPoint();
                                    ConfirmOkOnlyAsync(documentBundlePrefab, "視界を全て開放しました", scope).Forget();
                                }
                            );
                        },
                        element =>
                        {
                            GameListView.ApplyAsSimpleElement
                            (
                                element,
                                "プレイヤーの無敵フラグ",
                                _ =>
                                {
                                    AudioManager.PlaySfx("Sfx.Message.0");
                                    battleDebugData.IsInvinciblePlayer = !battleDebugData.IsInvinciblePlayer;
                                    ConfirmOkOnlyAsync(documentBundlePrefab, $"プレイヤーの無敵フラグを{battleDebugData.IsInvinciblePlayer}に設定しました", scope).Forget();
                                }
                            );
                        },
                        element =>
                        {
                            GameListView.ApplyAsSimpleElement
                            (
                                element,
                                "敵の無敵フラグ",
                                _ =>
                                {
                                    AudioManager.PlaySfx("Sfx.Message.0");
                                    battleDebugData.IsInvincibleEnemy = !battleDebugData.IsInvincibleEnemy;
                                    ConfirmOkOnlyAsync(documentBundlePrefab, $"敵の無敵フラグを{battleDebugData.IsInvincibleEnemy}に設定しました", scope).Forget();
                                }
                            );
                        },
                        element =>
                        {
                            GameListView.ApplyAsSimpleElement
                            (
                                element,
                                "全てのスキルの使用フラグ",
                                _ =>
                                {
                                    AudioManager.PlaySfx("Sfx.Message.0");
                                    battleDebugData.IsAllSkillAvailable = !battleDebugData.IsAllSkillAvailable;
                                    ConfirmOkOnlyAsync(documentBundlePrefab, $"全てのスキルの使用フラグを{battleDebugData.IsAllSkillAvailable}に設定しました", scope).Forget();
                                }
                            );
                        },
                        element =>
                        {
                            GameListView.ApplyAsSimpleElement
                            (
                                element,
                                "スキルコスト無し",
                                _ =>
                                {
                                    AudioManager.PlaySfx("Sfx.Message.0");
                                    battleDebugData.NoCost = !battleDebugData.NoCost;
                                    ConfirmOkOnlyAsync(documentBundlePrefab, $"スキルコスト無しを{battleDebugData.NoCost}に設定しました", scope).Forget();
                                }
                            );
                        },
                        element =>
                        {
                            GameListView.ApplyAsSimpleElement
                            (
                                element,
                                "大ダメージ",
                                _ =>
                                {
                                    AudioManager.PlaySfx("Sfx.Message.0");
                                    battleDebugData.IsBigDamage = !battleDebugData.IsBigDamage;
                                    ConfirmOkOnlyAsync(documentBundlePrefab, $"大ダメージを{battleDebugData.IsBigDamage}に設定しました", scope).Forget();
                                }
                            );
                        },
                        element =>
                        {
                            GameListView.ApplyAsSimpleElement
                            (
                                element,
                                "行動ポイント加算",
                                _ =>
                                {
                                    AudioManager.PlaySfx("Sfx.Message.0");
                                    player.GrowthParameter.BehaviourPoint += 1;
                                    ConfirmOkOnlyAsync(documentBundlePrefab, $"行動ポイントを{player.GrowthParameter.BehaviourPoint}に設定しました", scope).Forget();
                                }
                            );
                        },
                        element =>
                        {
                            GameListView.ApplyAsSimpleElement
                            (
                                element,
                                "チェックポイント呼び出し",
                                _ =>
                                {
                                    AudioManager.PlaySfx("Sfx.Message.0");
                                    var view = new GameSavePointMenuView(documentBundlePrefab, player);
                                    view.OpenAsync().Forget();
                                    source.TrySetResult();
                                }
                            );
                        },
                        element =>
                        {
                            GameListView.ApplyAsSimpleElement
                            (
                                element,
                                "ダンジョン変更",
                                _ =>
                                {
                                    AudioManager.PlaySfx("Sfx.Message.0");
                                    stateMachine.Change(StateSelectDungeonAsync);
                                }
                            );
                        },
                        element =>
                        {
                            GameListView.ApplyAsSimpleElement
                            (
                                element,
                                "エンカウント無し",
                                _ =>
                                {
                                    AudioManager.PlaySfx("Sfx.Message.0");
                                    battleDebugData.NoEncount = !battleDebugData.NoEncount;
                                    ConfirmOkOnlyAsync(documentBundlePrefab, $"エンカウント無しを{battleDebugData.NoEncount}に設定しました", scope).Forget();
                                }
                            );
                        },
                        element =>
                        {
                            GameListView.ApplyAsSimpleElement
                            (
                                element,
                                "スターターパック",
                                _ =>
                                {
                                    AudioManager.PlaySfx("Sfx.Message.0");
                                    stateMachine.Change(StateSelectStarterPackAsync);
                                }
                            );
                        },
                        element =>
                        {
                            GameListView.ApplyAsSimpleElement
                            (
                                element,
                                "想定経験値獲得",
                                _ =>
                                {
                                    AudioManager.PlaySfx("Sfx.Message.0");
                                    stateMachine.Change(StateSelectDungeonExperienceAsync);
                                }
                            );
                        },
                    },
                    0
                );
                await UniTask.WaitUntilCanceled(scope);
                UnityEngine.Object.Destroy(listDocument.gameObject);
            }

            async UniTask StateSelectBattleAsync(CancellationToken scope)
            {
                inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                    .Subscribe(_ =>
                    {
                        AudioManager.PlaySfx("Sfx.Cancel.0");
                        stateMachine.Change(StateRootAsync);
                    })
                    .RegisterTo(scope);
                var listElements = TinyServiceLocator.Resolve<MasterData>().Enemies.List
                    .Select(x => new Action<HKUIDocument>
                        (
                            element =>
                            {
                                GameListView.ApplyAsSimpleElement
                                (
                                    element,
                                    $"{x.Id}: {x.Name}",
                                    _ =>
                                    {
                                        AudioManager.PlaySfx("Sfx.Message.0");
                                        dungeonController.BeginBattleAsync(player, x).Forget();
                                        source.TrySetResult();
                                    }
                                );
                            }
                        )
                    );
                var listDocument = CreateList(documentBundlePrefab, listElements, 0);
                await UniTask.WaitUntilCanceled(scope);
                UnityEngine.Object.Destroy(listDocument.gameObject);
            }

            async UniTask StateSelectDungeonAsync(CancellationToken scope)
            {
                inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                    .Subscribe(_ =>
                    {
                        AudioManager.PlaySfx("Sfx.Cancel.0");
                        stateMachine.Change(StateRootAsync);
                    })
                    .RegisterTo(scope);
                var listElements = TinyServiceLocator.Resolve<MasterData>().DungeonSpecs.List
                    .Select(x => new Action<HKUIDocument>
                        (
                            element =>
                            {
                                GameListView.ApplyAsSimpleElement
                                (
                                    element,
                                    $"{x.Id}",
                                    _ =>
                                    {
                                        AudioManager.PlaySfx("Sfx.Message.0");
                                        dungeonController.Setup(x.Id, player);
                                        source.TrySetResult();
                                    }
                                );
                            }
                        )
                    );
                var listDocument = CreateList(documentBundlePrefab, listElements, 0);
                await UniTask.WaitUntilCanceled(scope);
                UnityEngine.Object.Destroy(listDocument.gameObject);
            }

            async UniTask StateSelectStarterPackAsync(CancellationToken scope)
            {
                inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                    .Subscribe(_ =>
                    {
                        AudioManager.PlaySfx("Sfx.Cancel.0");
                        stateMachine.Change(StateRootAsync);
                    })
                    .RegisterTo(scope);
                var dungeonLevels = new[] { 1, 2, 3 };
                var listElements = dungeonLevels
                    .Select(x => new Action<HKUIDocument>
                        (
                            element =>
                            {
                                GameListView.ApplyAsSimpleElement
                                (
                                    element,
                                    $"ダンジョンレベル: {x}",
                                    _ =>
                                    {
                                        AudioManager.PlaySfx("Sfx.Message.0");
                                        player.Inventory.Clear();
                                        player.Equipment.Clear();
                                        var itemTables = TinyServiceLocator.Resolve<MasterData>().ItemTables.Get(x)
                                            .OrderBy(y => UnityEngine.Random.value)
                                            .Take(15);
                                        foreach (var i in itemTables)
                                        {
                                            player.Inventory.Add(i.ItemId, 1);
                                            player.Events.OnAcquiredItem.OnNext((i.ItemId, 1));
                                        }
                                        ConfirmOkOnlyAsync(documentBundlePrefab, $"スターターパック{x}を追加しました", scope).Forget();
                                    }
                                );
                            }
                        )
                    );
                var listDocument = CreateList(documentBundlePrefab, listElements, 0);
                await UniTask.WaitUntilCanceled(scope);
                UnityEngine.Object.Destroy(listDocument.gameObject);
            }

            async UniTask StateSelectDungeonExperienceAsync(CancellationToken scope)
            {
                inputController.InputActions.UI.Cancel.OnPerformedAsObservable()
                    .Subscribe(_ =>
                    {
                        AudioManager.PlaySfx("Sfx.Cancel.0");
                        stateMachine.Change(StateRootAsync);
                    })
                    .RegisterTo(scope);
                var masterData = TinyServiceLocator.Resolve<MasterData>();
                var dungeonLevels = new[] { 1, 2, 3 };
                var listElements = masterData.DungeonSpecs.List
                    .Select(x => new Action<HKUIDocument>
                        (
                            element =>
                            {
                                GameListView.ApplyAsSimpleElement
                                (
                                    element,
                                    $"{x.Id}",
                                    async _ =>
                                    {
                                        var isAcquireBossExperience = await ConfirmAsync(documentBundlePrefab, "ボスの経験値も獲得しますか？", new[] { "はい", "いいえ" }, scope);
                                        AudioManager.PlaySfx("Sfx.Message.0");
                                        var result = 0;
                                        var max = UnityEngine.Random.Range(x.EnemyPlaceItemNumberMin, x.EnemyPlaceItemNumberMax);
                                        for (var i = 0; i < max; i++)
                                        {
                                            var enemyTableId = x.FloorItemEnemyPlaces[UnityEngine.Random.Range(0, x.FloorItemEnemyPlaces.Count)].EnemyTableId;
                                            var enemy = masterData.EnemyTables.Get(enemyTableId).Lottery().EnemyId.GetMasterDataEnemy();
                                            result += enemy.Experience;
                                        }
                                        if (isAcquireBossExperience == 0)
                                        {
                                            foreach (var i in x.FloorEnemyGuaranteeds)
                                            {
                                                var enemy = masterData.EnemyTables.Get(i.EnemyTableId).Lottery().EnemyId.GetMasterDataEnemy();
                                                result += enemy.Experience;
                                            }
                                        }
                                        max = UnityEngine.Random.Range(x.NoCostEnemyNumberMin, x.NoCostEnemyNumberMax);
                                        for (var i = 0; i < max; i++)
                                        {
                                            var enemyTableId = x.FloorEnemyNoCosts[UnityEngine.Random.Range(0, x.FloorEnemyNoCosts.Count)].EnemyTableId;
                                            var enemy = masterData.EnemyTables.Get(enemyTableId).Lottery().EnemyId.GetMasterDataEnemy();
                                            result += enemy.Experience;
                                        }
                                        player.InstanceStatus.AddExperience(result);
                                        ConfirmOkOnlyAsync(documentBundlePrefab, $"想定経験値\"{result}\"を獲得しました。", scope).Forget();
                                    }
                                );
                            }
                        )
                    );
                var listDocument = CreateList(documentBundlePrefab, listElements, 0);
                await UniTask.WaitUntilCanceled(scope);
                UnityEngine.Object.Destroy(listDocument.gameObject);
            }

            static HKUIDocument CreateList
            (
                HKUIDocument documentBundlePrefab,
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

            static UniTask ConfirmOkOnlyAsync
            (
                HKUIDocument documentBundlePrefab,
                string message,
                CancellationToken scope
            )
            {
                return DialogView.ConfirmAsync(documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Dialog"), message, new[] { "OK" }, 0, scope);
            }

            static UniTask<int> ConfirmAsync
            (
                HKUIDocument documentBundlePrefab,
                string message,
                string[] buttons,
                CancellationToken scope
            )
            {
                return DialogView.ConfirmAsync(documentBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Dialog"), message, buttons, 0, scope);
            }
        }

    }
#endif
}
