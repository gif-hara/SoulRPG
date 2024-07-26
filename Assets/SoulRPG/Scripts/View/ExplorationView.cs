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
            var dungeonController = TinyServiceLocator.Resolve<DungeonController>();
            SetupMiniMap(uiDocument, dungeonController, character, scope);
            SetupDungeon(dungeonController);
            SetupMessage(uiDocument);
        }

        private void SetupMiniMap(
            HKUIDocument uiDocument,
            DungeonController dungeonController,
            Character character,
            CancellationToken scope
            )
        {
            var positionText = uiDocument.Q<TMP_Text>("Text.Position");
            var areaDocument = uiDocument.Q<HKUIDocument>("Area.MiniMap");
            var areaTransform = uiDocument.Q<RectTransform>("Area.MiniMap");
            var size = areaDocument.Q<RectTransform>("Area.Tips").rect.size;
            var tipSize = size / 10;
            var tipsParent = areaDocument.Q<RectTransform>("Area.Tips.Viewport");
            var characterAreaTransform = areaDocument.Q<RectTransform>("Area.Character");
            var miniMapWallTopPrefab = areaDocument.Q<RectTransform>("UIElement.MapTip.Wall.Top");
            var miniMapWallLeftPrefab = areaDocument.Q<RectTransform>("UIElement.MapTip.Wall.Left");
            characterAreaTransform.sizeDelta = tipSize;
            character.PositionAsObservable()
                .Subscribe(x =>
                {
                    positionText.text = $"{x}";
                    tipsParent.anchoredPosition = new Vector2(-x.x * tipSize.x, -x.y * tipSize.y);
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

            foreach (var i in dungeonController.CurrentDungeon.wall.List)
            {
                var isHorizontal = i.a.y == i.b.y;
                var prefab = isHorizontal ? miniMapWallTopPrefab : miniMapWallLeftPrefab;
                var wallObject = Object.Instantiate(prefab, tipsParent.transform);
                wallObject.anchoredPosition = new Vector2(i.a.x * tipSize.x, i.a.y * tipSize.y);
                wallObject.sizeDelta = tipSize;
            }
        }

        private void SetupDungeon(
            DungeonController dungeonController
            )
        {
            var dungeonDocument = Object.Instantiate(dungeonDocumentPrefab);
            for (var i = 0; i <= dungeonController.CurrentDungeon.range.x; i++)
            {
                for (var j = 0; j <= dungeonController.CurrentDungeon.range.y; j++)
                {
                    var floorObject = Object.Instantiate(dungeonDocument.Q<Transform>("Dungeon.Floor"), dungeonDocument.transform);
                    floorObject.position = new Vector3(i, 0, j);
                }
            }
            foreach (var i in dungeonController.CurrentDungeon.wall.List)
            {
                var isHorizontal = i.a.y == i.b.y;
                var prefab = isHorizontal ? dungeonDocument.Q<Transform>("Dungeon.Wall.Top") : dungeonDocument.Q<Transform>("Dungeon.Wall.Left");
                var wallObject = Object.Instantiate(prefab, dungeonDocument.transform);
                wallObject.position = new Vector3(i.a.x, 0, i.a.y);
            }
        }

        private void SetupMessage(
            HKUIDocument uiDocument
            )
        {
            var areaDocument = uiDocument.Q<HKUIDocument>("Area.Message");
            var messageParent = areaDocument.Q<RectTransform>("Area.Text");
            var messagePrefab = areaDocument.Q<TMP_Text>("UIElement.Message");
        }
    }
}
