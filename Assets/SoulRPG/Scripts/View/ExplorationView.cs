using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.CharacterControllers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ExplorationView : IExplorationView
    {
        private readonly HKUIDocument uiDocumentPrefab;

        private readonly HKUIDocument dungeonDocumentPrefab;

        private readonly GameCameraController gameCameraController;

        private readonly Character character;

        private readonly Dictionary<(string dungeonName, int x, int y), GameObject> dungeonFloorEventObjects = new();

        private readonly Dictionary<(string dungeonName, int x, int y), GameObject> maptipFloorEventObjects = new();

        private readonly Dictionary<MasterData.WallEvent, HKUIDocument> maptipWallEventObjects = new();

        private readonly Dictionary<MasterData.WallEvent, HKUIDocument> dungeonWallEventObjects = new();

        private readonly Dictionary<Vector2Int, GameObject> maptipShadowObjects = new();

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
            var floorEvents = TinyServiceLocator.Resolve<MasterData>().FloorEvents.List
                .Where(x => x.DungeonName == dungeonController.CurrentDungeon.name);
            var wallEvents = TinyServiceLocator.Resolve<MasterData>().WallEvents.List
                .Where(x => x.DungeonName == dungeonController.CurrentDungeon.name);
            SetupMiniMap(uiDocument, dungeonController, floorEvents, wallEvents, character, scope);
            SetupDungeon(dungeonController, floorEvents, wallEvents);
            SetupMessage(uiDocument, character, scope);
            SetupStatuses(uiDocument, character, scope);
        }

        private void SetupMiniMap(
            HKUIDocument uiDocument,
            DungeonController dungeonController,
            IEnumerable<MasterData.FloorEvent> floorEvents,
            IEnumerable<MasterData.WallEvent> wallEvents,
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
            var shadowParent = areaDocument.Q<RectTransform>("Area.Shadow.Viewport");
            characterAreaTransform.sizeDelta = tipSize;
            character.PositionAsObservable()
                .Subscribe(x =>
                {
                    positionText.text = $"{x}";
                    var viewportPosition = new Vector2(-x.x * tipSize.x, -x.y * tipSize.y);
                    tipsParent.anchoredPosition = viewportPosition;
                    shadowParent.anchoredPosition = viewportPosition;
                    gameCameraController.transform.position = new Vector3(x.x, 0, x.y);
                    if (maptipShadowObjects.TryGetValue(x, out var shadowObject))
                    {
                        Object.Destroy(shadowObject);
                        maptipShadowObjects.Remove(x);
                    }
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

            for (var y = 0; y <= dungeonController.CurrentDungeon.range.y; y++)
            {
                for (var x = 0; x <= dungeonController.CurrentDungeon.range.x; x++)
                {
                    var position = new Vector2Int(x, y);
                    if (character.Position == position)
                    {
                        continue;
                    }
                    var shadowObject = Object.Instantiate(areaDocument.Q<RectTransform>("UIElement.MapTip.Shadow"), shadowParent);
                    shadowObject.anchoredPosition = new Vector2(x * tipSize.x, y * tipSize.y);
                    shadowObject.sizeDelta = tipSize;
                    maptipShadowObjects.Add(position, shadowObject.gameObject);
                }
            }

            CreateFloorEventObjects(floorEvents);
            CreateWallEventObjects(wallEvents);
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            gameEvents.OnAcquiredDungeonEvent
                .Subscribe(x =>
                {
                    if (maptipFloorEventObjects.TryGetValue(x, out var obj))
                    {
                        Object.Destroy(obj);
                        maptipFloorEventObjects.Remove(x);
                    }
                });
            gameEvents.OnClearTemporaryCompletedEventIds
                .Subscribe(x =>
                {
                    CreateFloorEventObjects(
                        x.Select(y => y.GetMasterDataDungeonEvent())
                    );
                });

            void CreateFloorEventObjects(IEnumerable<MasterData.FloorEvent> floorEvents)
            {
                foreach (var i in floorEvents)
                {
                    var eventObject = Object.Instantiate(areaDocument.Q<RectTransform>($"UIElement.MapTip.Floor.Event.{i.EventType}"), tipsParent.transform);
                    eventObject.anchoredPosition = new Vector2(i.X * tipSize.x, i.Y * tipSize.y);
                    eventObject.sizeDelta = tipSize;
                    maptipFloorEventObjects.Add((dungeonController.CurrentDungeon.name, i.X, i.Y), eventObject.gameObject);
                }
            }

            void CreateWallEventObjects(IEnumerable<MasterData.WallEvent> wallEvents)
            {
                foreach (var i in wallEvents)
                {
                    var isHorizontal = i.LeftY == i.RightY;
                    var directionName = isHorizontal ? "Top" : "Left";
                    var element = Object.Instantiate(areaDocument.Q<HKUIDocument>($"UIElement.MapTip.Wall.Event.{i.EventType}.{directionName}"), tipsParent.transform);
                    maptipWallEventObjects.Add(i, element);
                    var elementTransform = element.transform as RectTransform;
                    elementTransform.anchoredPosition = new Vector2(i.LeftX * tipSize.x, i.LeftY * tipSize.y);
                    elementTransform.sizeDelta = tipSize;
                    var isUnlock = TinyServiceLocator.Resolve<UserData>().ContainsCompletedWallEventId(i.Id);
                    element.Q("Open").SetActive(isUnlock);
                    element.Q("Close").SetActive(!isUnlock);
                }
            }
        }

        private void SetupDungeon(
            DungeonController dungeonController,
            IEnumerable<MasterData.FloorEvent> floorEvents,
            IEnumerable<MasterData.WallEvent> wallEvents
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

            CreateFloorEventObjects(floorEvents);
            CreateWallEventObjects(wallEvents);
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            gameEvents.OnAcquiredDungeonEvent
                .Subscribe(x =>
                {
                    if (dungeonFloorEventObjects.TryGetValue(x, out var obj))
                    {
                        Object.Destroy(obj);
                        dungeonFloorEventObjects.Remove(x);
                    }
                });
            gameEvents.OnClearTemporaryCompletedEventIds
                .Subscribe(x =>
                {
                    CreateFloorEventObjects(
                        x.Select(y => y.GetMasterDataDungeonEvent())
                    );
                });
            void CreateFloorEventObjects(IEnumerable<MasterData.FloorEvent> floorEvents)
            {
                foreach (var i in floorEvents)
                {
                    var eventObject = Object.Instantiate(dungeonDocument.Q<Transform>($"Dungeon.Floor.Event.{i.EventType}"), dungeonDocument.transform);
                    eventObject.position = new Vector3(i.X, 0, i.Y);
                    dungeonFloorEventObjects.Add((dungeonController.CurrentDungeon.name, i.X, i.Y), eventObject.gameObject);
                }
            }
            void CreateWallEventObjects(IEnumerable<MasterData.WallEvent> wallEvents)
            {
                foreach (var i in wallEvents)
                {
                    var isHorizontal = i.LeftY == i.RightY;
                    var directionName = isHorizontal ? "Top" : "Left";
                    var element = Object.Instantiate(dungeonDocument.Q<HKUIDocument>($"Dungeon.Wall.Event.{i.EventType}.{directionName}"), dungeonDocument.transform);
                    dungeonWallEventObjects.Add(i, element);
                    element.transform.position = new Vector3(i.LeftX, 0, i.LeftY);
                    var isUnlock = TinyServiceLocator.Resolve<UserData>().ContainsCompletedWallEventId(i.Id);
                    element.Q("Open").SetActive(isUnlock);
                    element.Q("Close").SetActive(!isUnlock);
                }
            }
        }

        private void SetupMessage(
            HKUIDocument uiDocument,
            Character character,
            CancellationToken scope
            )
        {
            var areaDocument = uiDocument.Q<HKUIDocument>("Area.Message");
            var messageParent = areaDocument.Q<RectTransform>("ListParent");
            var arrowObject = areaDocument.Q("Arrow");
            var messagePrefab = areaDocument.Q<HKUIDocument>("UIElement.Message");
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            arrowObject.SetActive(false);

            gameEvents.OnRequestShowMessage
                .Subscribe(x =>
                {
                    CreateElement(x);
                })
                .RegisterTo(scope);

            character.Events.OnAcquiredItem
                .Subscribe(x =>
                {
                    var masterDataItem = TinyServiceLocator.Resolve<MasterData>().Items.Get(x.itemId);
                    if (x.count == 1)
                    {
                        CreateElement($"{masterDataItem.Name}を手に入れた");
                    }
                    else
                    {
                        CreateElement($"{masterDataItem.Name}を{x.count}個手に入れた");
                    }
                })
                .RegisterTo(scope);

            gameEvents.OnRequestSetActiveMessageArrow
                .Subscribe(x =>
                {
                    arrowObject.SetActive(x);
                })
                .RegisterTo(scope);

            void CreateElement(string message)
            {
                var element = Object.Instantiate(messagePrefab, messageParent);
                element.Q<TMP_Text>("Message").text = message;
            }
        }

        private void SetupStatuses(
            HKUIDocument uiDocument,
            Character character,
            CancellationToken scope
            )
        {
            var areaDocument = uiDocument.Q<HKUIDocument>("Area.Status");
            var hitPointGauge = areaDocument.Q<Slider>("Gauge.HitPoint");
            Observable.Merge(
                character.InstanceStatus.HitPointAsObservable(),
                character.InstanceStatus.HitPointMaxAsObservable()
                )
                .Subscribe(_ =>
                {
                    hitPointGauge.value = character.InstanceStatus.HitPoint / (float)character.InstanceStatus.HitPointMax;
                })
                .RegisterTo(scope);
            var staminaGauge = areaDocument.Q<Slider>("Gauge.Stamina");
            Observable.Merge(
                character.InstanceStatus.StaminaAsObservable(),
                character.InstanceStatus.StaminaMaxAsObservable()
                )
                .Subscribe(_ =>
                {
                    staminaGauge.value = character.InstanceStatus.Stamina / (float)character.InstanceStatus.StaminaMax;
                })
                .RegisterTo(scope);
        }

        public UniTask OnOpenDoorAsync(MasterData.WallEvent wallEvent)
        {
            if (maptipWallEventObjects.TryGetValue(wallEvent, out var element))
            {
                element.Q("Open").SetActive(true);
                element.Q("Close").SetActive(false);
            }
            if (dungeonWallEventObjects.TryGetValue(wallEvent, out var dungeonElement))
            {
                dungeonElement.Q("Open").SetActive(true);
                dungeonElement.Q("Close").SetActive(false);
            }
            return UniTask.CompletedTask;
        }
    }
}
