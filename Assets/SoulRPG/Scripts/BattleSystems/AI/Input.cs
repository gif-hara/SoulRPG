using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using SoulRPG.BattleSystems.CommandInvokers;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class Input : IBattleAI
    {
        private readonly TinyStateMachine stateMachine = new();

        private readonly CommandView commandView;

        private readonly UniTaskCompletionSource<ICommandInvoker> source = new();

        public Input(HKUIDocument commandDocumentPrefab)
        {
            commandView = new CommandView(commandDocumentPrefab);
        }

        public UniTask<ICommandInvoker> ThinkAsync(BattleCharacter character)
        {
            commandView.Open();
            stateMachine.Change(StateSelectMainCommandAsync);
            return source.Task;
        }

        private async UniTask StateSelectMainCommandAsync(CancellationToken scope)
        {
            var commands = new[]
            {
                "武器",
                "道具",
                "逃亡"
            };

            var index = await commandView.CreateCommandsAsync("選べ", commands, 0);
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            switch (index)
            {
                case 0:
                    gameEvents.OnRequestShowMessage.OnNext("どうやら未実装のようだ");
                    stateMachine.Change(StateSelectMainCommandAsync);
                    break;
                case 1:
                    gameEvents.OnRequestShowMessage.OnNext("どうやら未実装のようだ");
                    stateMachine.Change(StateSelectMainCommandAsync);
                    break;
                case 2:
                    gameEvents.OnRequestShowMessage.OnNext("どうやら未実装のようだ");
                    stateMachine.Change(StateSelectMainCommandAsync);
                    break;
            }
        }
    }
}
