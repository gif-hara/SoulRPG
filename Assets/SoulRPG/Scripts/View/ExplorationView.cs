using System.Collections.Generic;
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

        private readonly Dictionary<DungeonInstanceFloorData, GameObject> dungeonFloorEventObjects = new();

        private readonly Dictionary<DungeonInstanceFloorData, GameObject> maptipFloorEventObjects = new();

        private readonly Dictionary<DungeonInstanceWallData, HKUIDocument> maptipWallEventObjects = new();

        private readonly Dictionary<DungeonInstanceWallData, HKUIDocument> dungeonWallEventObjects = new();

        private readonly List<GameObject> dungeonObjects = new();

        private readonly Dictionary<Vector2Int, GameObject> maptipShadowObjects = new();

        private HKUIDocument uiDocument;

        private readonly List<GameObject> messageElements = new();

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
            uiDocument = Object.Instantiate(uiDocumentPrefab);
            SetupMiniMap(uiDocument, character, scope);
            SetupDungeon(scope);
            SetupMessage(uiDocument, scope);
            SetupStatuses(uiDocument, character, scope);
            TinyServiceLocator.Resolve<GameEvents>().OnSetupDungeon
                .Subscribe(_ =>
                {
                    CreateDungeonViews();
                    foreach (var i in messageElements)
                    {
                        Object.Destroy(i);
                    }
                    messageElements.Clear();
                })
                .RegisterTo(scope);
        }

        public void CreateDungeonViews()
        {
            foreach (var i in dungeonFloorEventObjects)
            {
                Object.Destroy(i.Value);
            }
            dungeonFloorEventObjects.Clear();
            foreach (var i in maptipFloorEventObjects)
            {
                Object.Destroy(i.Value);
            }
            maptipFloorEventObjects.Clear();
            foreach (var i in maptipWallEventObjects)
            {
                Object.Destroy(i.Value.gameObject);
            }
            maptipWallEventObjects.Clear();
            foreach (var i in dungeonWallEventObjects)
            {
                Object.Destroy(i.Value.gameObject);
            }
            dungeonWallEventObjects.Clear();
            foreach (var i in dungeonObjects)
            {
                Object.Destroy(i);
            }
            dungeonObjects.Clear();
            foreach (var i in maptipShadowObjects)
            {
                Object.Destroy(i.Value);
            }
            maptipShadowObjects.Clear();
            CreateMiniMap();
            CreateDungeon();
        }

        public void SetActive(bool isActive)
        {
            uiDocument.gameObject.SetActive(isActive);
        }

        public void BeginSubscribe(BattleCharacter battleCharacter, CancellationToken scope)
        {
            battleCharacter.Events.OnTakeDamage
                .Subscribe(_ =>
                {
                    uiDocument.Q<HKUIDocument>("Sequences")
                        .Q<SequenceMonobehaviour>("Animation.OnTakeDamage")
                        .PlayAsync(scope)
                        .Forget();
                })
                .RegisterTo(scope);
        }

        private void CreateMiniMap()
        {
            var dungeonController = TinyServiceLocator.Resolve<DungeonController>();
            var scope = dungeonController.DungeonScope;
            var areaDocument = uiDocument.Q<HKUIDocument>("Area.MiniMap");
            var miniMapWallPrefab = areaDocument.Q<RectTransform>("UIElement.MapTip.Wall");
            var tipsParent = areaDocument.Q<RectTransform>("Area.Tips.Viewport");
            var size = areaDocument.Q<RectTransform>("Area.Tips").rect.size;
            var tipSize = size / 10;
            var shadowParent = areaDocument.Q<RectTransform>("Area.Shadow.Viewport");

            foreach (var i in dungeonController.CurrentDungeon.wall.List)
            {
                var isHorizontal = i.a.y == i.b.y;
                var prefab = miniMapWallPrefab;
                var wallObject = Object.Instantiate(prefab, tipsParent.transform);
                wallObject.anchoredPosition = new Vector2(i.a.x * tipSize.x, i.a.y * tipSize.y);
                wallObject.sizeDelta = tipSize;
                wallObject.rotation = Quaternion.Euler(0, 0, isHorizontal ? 0 : 90);
                dungeonObjects.Add(wallObject.gameObject);
            }

            if (dungeonController.CurrentDungeonSpec.RequireShadow)
            {
                for (var y = 0; y < dungeonController.CurrentDungeon.range.y; y++)
                {
                    for (var x = 0; x < dungeonController.CurrentDungeon.range.x; x++)
                    {
                        if (dungeonController.ContainsReachedPoint(new Vector2Int(x, y)))
                        {
                            continue;
                        }
                        var position = new Vector2Int(x, y);
                        var shadowObject = Object.Instantiate(areaDocument.Q<RectTransform>("UIElement.MapTip.Shadow"), shadowParent);
                        shadowObject.anchoredPosition = new Vector2(x * tipSize.x, y * tipSize.y);
                        shadowObject.sizeDelta = tipSize;
                        maptipShadowObjects.Add(position, shadowObject.gameObject);
                    }
                }
            }

            foreach (var (position, floorData) in dungeonController.FloorDatabase)
            {
                var eventObject = Object.Instantiate(areaDocument.Q<RectTransform>($"UIElement.MapTip.Floor.Event.{floorData.ViewName}"), tipsParent.transform);
                eventObject.anchoredPosition = new Vector2(position.x * tipSize.x, position.y * tipSize.y);
                eventObject.sizeDelta = tipSize;
                maptipFloorEventObjects.Add(floorData, eventObject.gameObject);
            }
            foreach (var i in TinyServiceLocator.Resolve<DungeonController>().WallDatabase)
            {
                var isHorizontal = i.Key.From.y == i.Key.To.y;
                var element = Object.Instantiate(areaDocument.Q<HKUIDocument>($"UIElement.MapTip.Wall.Event.{i.Value.EventType}"), tipsParent.transform);
                maptipWallEventObjects.Add(i.Value, element);
                var elementTransform = element.transform as RectTransform;
                elementTransform.anchoredPosition = new Vector2(i.Key.From.x * tipSize.x, i.Key.From.y * tipSize.y);
                elementTransform.sizeDelta = tipSize;
                elementTransform.rotation = Quaternion.Euler(0, 0, isHorizontal ? 0 : 90);
                i.Value.IsOpenReactiveProperty
                    .Subscribe(x =>
                    {
                        element.Q("Open").SetActive(x);
                        element.Q("Close").SetActive(!x);
                    })
                    .RegisterTo(scope);
                dungeonObjects.Add(element.gameObject);
            }
            foreach (var i in dungeonController.Enemies)
            {
                var characterObject = Object.Instantiate(areaDocument.Q<RectTransform>("UIElement.MapTip.Enemy"), tipsParent.transform);
                characterObject.anchoredPosition = new Vector2(i.Position.x * tipSize.x, i.Position.y * tipSize.y);
                characterObject.sizeDelta = tipSize;
                i.PositionAsObservable()
                    .Subscribe(x =>
                    {
                        characterObject.anchoredPosition = new Vector2(x.x * tipSize.x, x.y * tipSize.y);
                    })
                    .RegisterTo(i.LifeScope);
                i.LifeScope.Register(() =>
                {
                    if (characterObject == null)
                    {
                        return;
                    }
                    Object.Destroy(characterObject.gameObject);
                });
                dungeonObjects.Add(characterObject.gameObject);
            }
        }

        private void SetupMiniMap(
            HKUIDocument uiDocument,
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
            var characterRotationObject = areaDocument.Q<RectTransform>("Character.RotationObject");
            var miniMapWallPrefab = areaDocument.Q<RectTransform>("UIElement.MapTip.Wall");
            var shadowParent = areaDocument.Q<RectTransform>("Area.Shadow.Viewport");
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            var miniMapType = Define.MiniMapType.Default;
            characterAreaTransform.sizeDelta = tipSize;
            character.PositionAsObservable()
                .Subscribe(x =>
                {
                    positionText.text = $"{x}";
                    var viewportPosition = new Vector2(-x.x * tipSize.x, -x.y * tipSize.y);
                    tipsParent.anchoredPosition = viewportPosition;
                    shadowParent.anchoredPosition = viewportPosition;
                    gameCameraController.transform.position = new Vector3(x.x, 0, x.y);
                })
                .RegisterTo(scope);
            character.DirectionAsObservable()
                .Subscribe(x =>
                {
                    characterRotationObject.rotation = Quaternion.Euler(0, 0, -x.ToAngle());
                    gameCameraController.transform.rotation = Quaternion.Euler(0, x.ToAngle(), 0);
                })
                .RegisterTo(scope);
            gameEvents.OnRequestChangeMiniMapType
                .Subscribe(x =>
                {
                    ChangeViewType(x);
                })
                .RegisterTo(scope);
            gameEvents.OnRequestToggleMiniMapType
                .Subscribe(_ =>
                {
                    ChangeViewType(miniMapType == Define.MiniMapType.Default ? Define.MiniMapType.View : Define.MiniMapType.Default);
                })
                .RegisterTo(scope);
            gameEvents.OnAddReachedPoint
                .Subscribe(RemoveShadow)
                .RegisterTo(scope);
            gameEvents.OnAcquiredFloorData
                .Subscribe(x =>
                {
                    if (maptipFloorEventObjects.TryGetValue(x, out var obj))
                    {
                        Object.Destroy(obj);
                        maptipFloorEventObjects.Remove(x);
                    }
                })
                .RegisterTo(scope);
            void RemoveShadow(Vector2Int position)
            {
                if (maptipShadowObjects.TryGetValue(position, out var shadowObject))
                {
                    Object.Destroy(shadowObject);
                    maptipShadowObjects.Remove(position);
                }
            }

            void ChangeViewType(Define.MiniMapType type)
            {
                miniMapType = type;
                var miniMapStyleDocument = uiDocument.Q<HKUIDocument>($"MiniMapStyles");
                var styleRectTransform = miniMapStyleDocument.Q<RectTransform>(type.ToString());
                areaTransform.SetParent(styleRectTransform, false);
            }
        }

        private void CreateDungeon()
        {
            var dungeonDocument = Object.Instantiate(dungeonDocumentPrefab);
            var dungeonController = TinyServiceLocator.Resolve<DungeonController>();
            for (var i = 0; i <= dungeonController.CurrentDungeon.range.x; i++)
            {
                for (var j = 0; j <= dungeonController.CurrentDungeon.range.y; j++)
                {
                    var floorObject = Object.Instantiate(dungeonDocument.Q<Transform>("Dungeon.Floor"), dungeonDocument.transform);
                    floorObject.position = new Vector3(i, 0, j);
                    floorObject.rotation = Quaternion.Euler(0.0f, (i + j) % 2 * 90.0f, 0.0f);
                    dungeonObjects.Add(floorObject.gameObject);
                }
            }
            foreach (var i in dungeonController.CurrentDungeon.wall.List)
            {
                var isHorizontal = i.a.y == i.b.y;
                var prefab = dungeonDocument.Q<Transform>("Dungeon.Wall");
                var wallObject = Object.Instantiate(prefab, dungeonDocument.transform);
                wallObject.position = new Vector3(i.a.x, 0, i.a.y);
                wallObject.rotation = Quaternion.Euler(0, isHorizontal ? 0 : -90, 0);
                dungeonObjects.Add(wallObject.gameObject);
            }

            foreach (var (position, floorData) in dungeonController.FloorDatabase)
            {
                var eventObject = Object.Instantiate(dungeonDocument.Q<Transform>($"Dungeon.Floor.Event.{floorData.ViewName}"), dungeonDocument.transform);
                eventObject.position = new Vector3(position.x, 0, position.y);
                dungeonFloorEventObjects.Add(floorData, eventObject.gameObject);
            }
            foreach (var i in dungeonController.WallDatabase)
            {
                var isHorizontal = i.Key.From.y == i.Key.To.y;
                var element = Object.Instantiate(dungeonDocument.Q<HKUIDocument>($"Dungeon.Wall.Event.{i.Value.EventType}"), dungeonDocument.transform);
                dungeonWallEventObjects.Add(i.Value, element);
                element.transform.position = new Vector3(i.Key.From.x, 0, i.Key.From.y);
                element.transform.rotation = Quaternion.Euler(0, isHorizontal ? 0 : -90, 0);
                i.Value.IsOpenReactiveProperty
                    .Subscribe(x =>
                    {
                        element.Q("Open").SetActive(x);
                        element.Q("Close").SetActive(!x);
                    });
            }
            foreach (var i in dungeonController.Enemies)
            {
                var characterObject = Object.Instantiate(dungeonDocument.Q<Transform>("Dungeon.Enemy"), dungeonDocument.transform);
                characterObject.position = new Vector3(i.Position.x, 0, i.Position.y);
                i.PositionAsObservable()
                    .Subscribe(x =>
                    {
                        characterObject.position = new Vector3(x.x, 0, x.y);
                    })
                    .RegisterTo(i.LifeScope);
                i.LifeScope.Register(() =>
                {
                    if (characterObject == null)
                    {
                        return;
                    }
                    Object.Destroy(characterObject.gameObject);
                });
                dungeonObjects.Add(characterObject.gameObject);
            }
        }

        private void SetupDungeon(CancellationToken scope)
        {
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            gameEvents.OnAcquiredFloorData
                .Subscribe(x =>
                {
                    if (dungeonFloorEventObjects.TryGetValue(x, out var obj))
                    {
                        Object.Destroy(obj);
                        dungeonFloorEventObjects.Remove(x);
                    }
                })
                .RegisterTo(scope);
        }

        private void SetupMessage(
            HKUIDocument uiDocument,
            CancellationToken scope
            )
        {
            var areaDocument = uiDocument.Q<HKUIDocument>("Area.Message");
            var messageParent = areaDocument.Q<RectTransform>("ListParent");
            var arrowObject = areaDocument.Q("Area.Arrow");
            var messagePrefab = areaDocument.Q<HKUIDocument>("UIElement.Message");
            var arrowAnimation = uiDocument
                .Q<HKUIDocument>("Sequences")
                .Q<SequenceMonobehaviour>("Animation.Arrow");
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            CancellationTokenSource arrowAnimationScope = null;
            arrowObject.SetActive(false);

            gameEvents.OnRequestShowMessage
                .Subscribe(x =>
                {
                    CreateElement(x.Message);
                })
                .RegisterTo(scope);

            gameEvents.OnRequestSetActiveMessageArrow
                .Subscribe(x =>
                {
                    arrowObject.SetActive(x);
                    arrowAnimationScope?.Cancel();
                    arrowAnimationScope?.Dispose();
                    arrowAnimationScope = null;
                    if (x)
                    {
                        arrowAnimationScope = new CancellationTokenSource();
                        arrowAnimation.PlayAsync(arrowAnimationScope.Token).Forget();
                    }
                })
                .RegisterTo(scope);

            void CreateElement(string message)
            {
                var element = Object.Instantiate(messagePrefab, messageParent);
                messageElements.Add(element.gameObject);
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
            var hitPointGaugeTransform = hitPointGauge.transform as RectTransform;
            var guardGauge = areaDocument.Q<Slider>("Gauge.Guard");
            var guardGaugeTransform = guardGauge.transform as RectTransform;
            var gameRule = TinyServiceLocator.Resolve<GameRule>();
            Observable.Merge(
                character.InstanceStatus.HitPointAsObservable(),
                character.InstanceStatus.HitPointMaxAsObservable()
                )
                .Subscribe(_ =>
                {
                    hitPointGauge.value = character.InstanceStatus.HitPoint / (float)character.InstanceStatus.HitPointMax;
                    var anchorMax = hitPointGaugeTransform.anchorMax;
                    anchorMax.x = character.InstanceStatus.HitPointMax * gameRule.HitPointGaugeAmount / (float)gameRule.HitPointMax;
                    anchorMax.x = Mathf.Clamp(anchorMax.x, 0, 1);
                    hitPointGaugeTransform.anchorMax = anchorMax;
                    guardGaugeTransform.anchorMax = anchorMax;
                })
                .RegisterTo(scope);
            character.InstanceStatus.GuardPointAsObservable()
                .Subscribe(x =>
                {
                    guardGauge.value = x / (float)character.InstanceStatus.HitPointMax;
                })
                .RegisterTo(scope);
            var staminaGauge = areaDocument.Q<Slider>("Gauge.Stamina");
            var staminaGaugeTransform = staminaGauge.transform as RectTransform;
            Observable.Merge(
                character.InstanceStatus.StaminaAsObservable(),
                character.InstanceStatus.StaminaMaxAsObservable()
                )
                .Subscribe(_ =>
                {
                    staminaGauge.value = character.InstanceStatus.Stamina / (float)character.InstanceStatus.StaminaMax;
                    var anchorMax = staminaGaugeTransform.anchorMax;
                    anchorMax.x = character.InstanceStatus.StaminaMax * gameRule.StaminaGaugeAmount / (float)gameRule.StaminaMax;
                    anchorMax.x = Mathf.Clamp(anchorMax.x, 0, 1);
                    staminaGaugeTransform.anchorMax = anchorMax;
                })
                .RegisterTo(scope);
            var experience = areaDocument.Q<TMP_Text>("Text.Experience");
            character.InstanceStatus.ExperienceAsObservable()
                .Subscribe(x =>
                {
                    experience.SetText(x.ToString());
                })
                .RegisterTo(scope);
        }

        public UniTask OnOpenDoorAsync(DungeonInstanceWallData wallData)
        {
            if (maptipWallEventObjects.TryGetValue(wallData, out var element))
            {
                element.Q("Open").SetActive(true);
                element.Q("Close").SetActive(false);
            }
            if (dungeonWallEventObjects.TryGetValue(wallData, out var dungeonElement))
            {
                dungeonElement.Q("Open").SetActive(true);
                dungeonElement.Q("Close").SetActive(false);
            }
            return UniTask.CompletedTask;
        }
    }
}
