using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    public sealed class ExplorationView
    {
        private readonly HKUIDocument uiDocumentPrefab;

        private readonly HKUIDocument dungeonDocumentPrefab;

        private readonly GameCameraController gameCameraController;

        private readonly Character character;

        private Dictionary<(string dungeonName, int x, int y), GameObject> dungeonEventObjects = new();

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
            SetupMessage(uiDocument, character, scope);
            SetupStatuses(uiDocument, character, scope);
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

            var events = TinyServiceLocator.Resolve<MasterData>().FloorEvents.List
                .Where(x => x.DungeonName == dungeonController.CurrentDungeon.name);
            CreateFloorEventObjects(events);
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            gameEvents.OnAcquiredDungeonEvent
                .Subscribe(x =>
                {
                    if (dungeonEventObjects.TryGetValue(x, out var obj))
                    {
                        Object.Destroy(obj);
                        dungeonEventObjects.Remove(x);
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
                    var eventObject = Object.Instantiate(dungeonDocument.Q<Transform>($"Dungeon.Event.{i.EventType}"), dungeonDocument.transform);
                    eventObject.position = new Vector3(i.X, 0, i.Y);
                    dungeonEventObjects.Add((dungeonController.CurrentDungeon.name, i.X, i.Y), eventObject.gameObject);
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
    }
}
