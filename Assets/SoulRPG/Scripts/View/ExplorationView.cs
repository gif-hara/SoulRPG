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
    public sealed class ExplorationView
    {
        private readonly HKUIDocument uiDocumentPrefab;

        private readonly HKUIDocument dungeonDocumentPrefab;

        private readonly GameCameraController gameCameraController;

        private readonly Character character;

        public ExplorationView(
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
            var dungeonDocument = Object.Instantiate(dungeonDocumentPrefab);
            var dungeonFloorObjects = new List<Transform>();
            var dungeonWallObjects = new List<Transform>();
            var dungeonController = TinyServiceLocator.Resolve<DungeonController>();
            SetupMiniMap(uiDocument, dungeonController, character, scope);

            foreach (var floor in dungeonFloorObjects)
            {
                Object.Destroy(floor.gameObject);
            }
            dungeonFloorObjects.Clear();
            for (var i = 0; i <= dungeonController.CurrentDungeon.range.x; i++)
            {
                for (var j = 0; j <= dungeonController.CurrentDungeon.range.y; j++)
                {
                    var floorObject = Object.Instantiate(dungeonDocument.Q<Transform>("Dungeon.Floor"), dungeonDocument.transform);
                    dungeonFloorObjects.Add(floorObject);
                    floorObject.position = new Vector3(i, 0, j);
                }
            }

            foreach (var wall in dungeonWallObjects)
            {
                Object.Destroy(wall.gameObject);
            }
            dungeonWallObjects.Clear();
            foreach (var i in dungeonController.CurrentDungeon.wall.List)
            {
                var isHorizontal = i.a.y == i.b.y;
                var prefab = isHorizontal ? dungeonDocument.Q<Transform>("Dungeon.Wall.Top") : dungeonDocument.Q<Transform>("Dungeon.Wall.Left");
                var wallObject = Object.Instantiate(prefab, dungeonDocument.transform);
                dungeonWallObjects.Add(wallObject);
                wallObject.position = new Vector3(i.a.x, 0, i.a.y);
            }
        }

        private void SetupMiniMap(
            HKUIDocument uiDocument,
            DungeonController dungeonController,
            Character character,
            CancellationToken scope
            )
        {
            var positionText = uiDocument.Q<TMP_Text>("Text.Position");
            var miniMapAreaDocument = uiDocument.Q<HKUIDocument>("Area.MiniMap");
            var miniMapAreaTransform = uiDocument.Q<RectTransform>("Area.MiniMap");
            var miniMapSize = miniMapAreaDocument.Q<RectTransform>("Area.Tips").rect.size;
            var miniMapTipSize = miniMapSize / 10;
            var miniMapTipsParent = miniMapAreaDocument.Q<RectTransform>("Area.Tips.Viewport");
            var characterAreaTransform = miniMapAreaDocument.Q<RectTransform>("Area.Character");
            var miniMapWallTopPrefab = miniMapAreaDocument.Q<RectTransform>("UIElement.MapTip.Wall.Top");
            var miniMapWallLeftPrefab = miniMapAreaDocument.Q<RectTransform>("UIElement.MapTip.Wall.Left");
            var miniMapWallObjects = new List<RectTransform>();
            characterAreaTransform.sizeDelta = miniMapTipSize;
            character.PositionAsObservable()
                .Subscribe(x =>
                {
                    positionText.text = $"{x}";
                    miniMapTipsParent.anchoredPosition = new Vector2(-x.x * miniMapTipSize.x, -x.y * miniMapTipSize.y);
                    gameCameraController.transform.position = new Vector3(x.x, 0, x.y);
                })
                .RegisterTo(scope);
            character.DirectionAsObservable()
                .Subscribe(x =>
                {
                    characterAreaTransform.rotation = Quaternion.Euler(0, 0, -x.ToAngle());
                    gameCameraController.transform.rotation = Quaternion.Euler(0, x.ToAngle(), 0);
                })
                .RegisterTo(scope);

            foreach (var wall in miniMapWallObjects)
            {
                Object.Destroy(wall.gameObject);
            }
            miniMapWallObjects.Clear();
            foreach (var i in dungeonController.CurrentDungeon.wall.List)
            {
                var isHorizontal = i.a.y == i.b.y;
                var prefab = isHorizontal ? miniMapWallTopPrefab : miniMapWallLeftPrefab;
                var wallObject = Object.Instantiate(prefab, miniMapTipsParent.transform);
                miniMapWallObjects.Add(wallObject);
                wallObject.anchoredPosition = new Vector2(i.a.x * miniMapTipSize.x, i.a.y * miniMapTipSize.y);
                wallObject.sizeDelta = miniMapTipSize;
            }
        }
    }
}
