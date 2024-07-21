using System.Threading;
using HK;
using R3;
using SoulRPG.CharacterControllers;
using TMPro;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameView
    {
        private readonly HKUIDocument documentPrefab;

        private readonly Character character;

        public GameView(HKUIDocument documentPrefab, Character character)
        {
            this.documentPrefab = documentPrefab;
            this.character = character;
        }

        public void Open(CancellationToken scope)
        {
            var document = Object.Instantiate(documentPrefab);
            var positionText = document.Q<TMP_Text>("Text.Position");
            var directionText = document.Q<TMP_Text>("Text.Direction");
            var miniMapAreaDocument = document.Q<HKUIDocument>("Area.MiniMap");
            var characterAreaTransform = miniMapAreaDocument.Q<Transform>("Area.Character");
            character.PositionAsObservable()
                .Subscribe(x =>
                {
                    positionText.text = $"Position: {x}";

                })
                .RegisterTo(scope);
            character.DirectionAsObservable()
                .Subscribe(x =>
                {
                    directionText.text = $"Direction: {x}";
                    characterAreaTransform.rotation = Quaternion.Euler(0, 0, -x.ToAngle());
                })
                .RegisterTo(scope);
        }
    }
}
