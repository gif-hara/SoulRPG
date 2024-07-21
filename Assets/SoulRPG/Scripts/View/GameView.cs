using System.Collections.Generic;
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
        private readonly HKUIDocument uiDocumentPrefab;

        private readonly HKUIDocument dungeonDocumentPrefab;

        private readonly GameCameraController gameCameraController;

        private readonly Character character;

        public GameView(
            HKUIDocument uiDocumentPrefab,
            HKUIDocument dungeonDocumentPrefab,
            GameCameraController gameCameraController,
            Character character
            )
        {
            this.uiDocumentPrefab = uiDocumentPrefab;
            this.dungeonDocumentPrefab = dungeonDocumentPrefab;
            this.gameCameraController = gameCameraController;
            this.character = character;
        }

        public void Open(CancellationToken scope)
        {
            var uiDocument = Object.Instantiate(uiDocumentPrefab);
            var positionText = uiDocument.Q<TMP_Text>("Text.Position");
            var directionText = uiDocument.Q<TMP_Text>("Text.Direction");
            var miniMapAreaDocument = uiDocument.Q<HKUIDocument>("Area.MiniMap");
            var miniMapTipsParent = miniMapAreaDocument.Q<RectTransform>("Area.Tips.Viewport");
            var characterAreaTransform = miniMapAreaDocument.Q<Transform>("Area.Character");
            var miniMapWallTopPrefab = miniMapAreaDocument.Q<RectTransform>("UIElement.MapTip.Wall.Top");
            var miniMapWallLeftPrefab = miniMapAreaDocument.Q<RectTransform>("UIElement.MapTip.Wall.Left");
            var miniMapWallObjects = new List<RectTransform>();
            var dungeonDocument = Object.Instantiate(dungeonDocumentPrefab);
            var dungeonFloorObjects = new List<Transform>();
            character.PositionAsObservable()
                .Subscribe(x =>
                {
                    positionText.text = $"Position: {x}";
                    miniMapTipsParent.anchoredPosition = new Vector2(-x.x * 100, -x.y * 100);
                    gameCameraController.transform.position = new Vector3(x.x, 0, x.y);
                })
                .RegisterTo(scope);
            character.DirectionAsObservable()
                .Subscribe(x =>
                {
                    directionText.text = $"Direction: {x}";
                    characterAreaTransform.rotation = Quaternion.Euler(0, 0, -x.ToAngle());
                    gameCameraController.transform.rotation = Quaternion.Euler(0, x.ToAngle(), 0);
                })
                .RegisterTo(scope);
            character.DungeonAsObservable()
                .Subscribe(x =>
                {
                    foreach (var wall in miniMapWallObjects)
                    {
                        Object.Destroy(wall.gameObject);
                    }
                    miniMapWallObjects.Clear();
                    foreach (var i in x.wall.List)
                    {
                        var isHorizontal = i.a.y == i.b.y;
                        var prefab = isHorizontal ? miniMapWallTopPrefab : miniMapWallLeftPrefab;
                        var wallObject = Object.Instantiate(prefab, miniMapTipsParent.transform);
                        miniMapWallObjects.Add(wallObject);
                        wallObject.anchoredPosition = new Vector2(i.a.x * 100, i.a.y * 100);
                    }

                    foreach (var floor in dungeonFloorObjects)
                    {
                        Object.Destroy(floor.gameObject);
                    }
                    dungeonFloorObjects.Clear();
                    for (var i = 0; i < x.range.x; i++)
                    {
                        for (var j = 0; j < x.range.y; j++)
                        {
                            var floorObject = Object.Instantiate(dungeonDocument.Q<Transform>("Dungeon.Floor"), dungeonDocument.transform);
                            dungeonFloorObjects.Add(floorObject);
                            floorObject.position = new Vector3(i, 0, j);
                        }
                    }
                })
                .RegisterTo(scope);
        }
    }
}
