using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using SoulRPG.CharacterControllers;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class OpenNameInputFieldView : ISequence
    {
        [SerializeField]
        private HKUIDocument gameMenuBundlePrefab;

        public async UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var saveData = SaveData.LoadSafe();
            var player = TinyServiceLocator.Resolve<Character>("Player");
            var inputController = TinyServiceLocator.Resolve<InputController>();
            inputController.PushInputType(InputController.InputType.UI);
            var newPlayerName = await GameNameInputFieldView.OpenAsync(
                gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.NameInputField"),
                gameMenuBundlePrefab.Q<HKUIDocument>("UI.Game.Menu.Dialog"),
                cancellationToken
            );
            inputController.PopInputType();
            saveData.playerData.name = newPlayerName;
            saveData.Save();
            player.Name = newPlayerName;
        }
    }
}
