using System;
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
        private readonly BattleCharacter character;

        private readonly HKUIDocument commandDocumentPrefab;

        public Input(BattleCharacter character, HKUIDocument commandDocumentPrefab)
        {
            this.character = character;
            this.commandDocumentPrefab = commandDocumentPrefab;
        }

        public UniTask<ICommandInvoker> ThinkAsync(BattleCharacter character)
        {
            return UniTask.FromResult(default(ICommandInvoker));
        }
    }
}
