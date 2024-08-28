using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.CharacterControllers;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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
                        AudioManager.PlaySFX("Sfx.Cancel.0");
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
                                    AudioManager.PlaySFX("Sfx.Message.0");
                                    foreach (var i in TinyServiceLocator.Resolve<MasterData>().Items.List)
                                    {
                                        player.Inventory.Add(i.Id, 1);
                                    }
                                    ConfirmOkOnlyAsync(documentBundlePrefab, "全アイテムを追加しました", scope).Forget();
                                }
                            );
                        },
                    },
                    0
                );
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
        }

    }
#endif
}
