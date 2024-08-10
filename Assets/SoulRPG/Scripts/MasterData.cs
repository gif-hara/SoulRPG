using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using HK;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [CreateAssetMenu(fileName = "MasterData", menuName = "SoulRPG/MasterData")]
    public sealed class MasterData : ScriptableObject
    {
        [SerializeField]
        private Dungeon.DictionaryList dungeons;
        public Dungeon.DictionaryList Dungeons => dungeons;

        [SerializeField]
        private DungeonSpec.DictionaryList dungeonSpecs;
        public DungeonSpec.DictionaryList DungeonSpecs => dungeonSpecs;

        [SerializeField]
        private FloorEventEnemy.DictionaryList floorEventEnemies;
        public FloorEventEnemy.DictionaryList FloorEventEnemies => floorEventEnemies;

        [SerializeField]
        private WallEventConditionItem.Group wallEventConditionItems;
        public WallEventConditionItem.Group WallEventConditionItems => wallEventConditionItems;

        [SerializeField]
        private Item.DictionaryList items;
        public Item.DictionaryList Items => items;

        [SerializeField]
        private ItemTable.Group itemTables;
        public ItemTable.Group ItemTables => itemTables;

        [SerializeField]
        private Weapon.DictionaryList weapons;
        public Weapon.DictionaryList Weapons => weapons;

        [SerializeField]
        private Skill.DictionaryList skills;
        public Skill.DictionaryList Skills => skills;

        [SerializeField]
        private Armor.DictionaryList armorHeads;
        public Armor.DictionaryList ArmorHeads => armorHeads;

        [SerializeField]
        private Armor.DictionaryList armorBodies;
        public Armor.DictionaryList ArmorBodies => armorBodies;

        [SerializeField]
        private Armor.DictionaryList armorArms;
        public Armor.DictionaryList ArmorArms => armorArms;

        [SerializeField]
        private Armor.DictionaryList armorLegs;
        public Armor.DictionaryList ArmorLegs => armorLegs;

        [SerializeField]
        private Accessory.DictionaryList accessories;
        public Accessory.DictionaryList Accessories => accessories;

        [SerializeField]
        private Enemy.DictionaryList enemies;
        public Enemy.DictionaryList Enemies => enemies;

        [SerializeField]
        private Ailment.DictionaryList ailments;
        public Ailment.DictionaryList Ailments => ailments;

        [SerializeField]
        private EnemyTable.Group enemyTables;
        public EnemyTable.Group EnemyTables => enemyTables;


#if UNITY_EDITOR
        [ContextMenu("Update")]
        private async void UpdateMasterData()
        {
            Debug.Log("Begin MasterData Update");
            var dungeonNames = new[]
            {
                "Dungeon.Test"
            };
            var masterDataNames = new[]
            {
                "MasterData.Item",
                "MasterData.Weapon",
                "MasterData.Skill",
                "MasterData.Armor.Head",
                "MasterData.Armor.Body",
                "MasterData.Armor.Arms",
                "MasterData.Armor.Legs",
                "MasterData.Accessory",
                "MasterData.Enemy",
                "MasterData.FloorEvent.Enemy",
                "MasterData.Ailment",
                "MasterData.Enemy.CharacterAttribute",
                "MasterData.WallEvent",
                "MasterData.WallEvent.Condition.Item",
                "MasterData.DungeonSpec",
                "MasterData.ItemTable",
                "MasterData.FloorItem.NoCost",
                "MasterData.FloorItem.EnemyPlace",
                "MasterData.EnemyTable"
            };
            var dungeonDownloader = UniTask.WhenAll(
                dungeonNames.Select(GoogleSpreadSheetDownloader.DownloadAsync)
            );
            var masterDataDownloader = UniTask.WhenAll(
                masterDataNames.Select(GoogleSpreadSheetDownloader.DownloadAsync)
            );
            var database = await UniTask.WhenAll(dungeonDownloader, masterDataDownloader);
            dungeons.Set(database.Item1.Select((x, i) => Dungeon.Create(dungeonNames[i], x)));
            items.Set(JsonHelper.FromJson<Item>(database.Item2[0]));
            weapons.Set(JsonHelper.FromJson<Weapon>(database.Item2[1]));
            skills.Set(JsonHelper.FromJson<Skill>(database.Item2[2]));
            armorHeads.Set(JsonHelper.FromJson<Armor>(database.Item2[3]));
            armorBodies.Set(JsonHelper.FromJson<Armor>(database.Item2[4]));
            armorArms.Set(JsonHelper.FromJson<Armor>(database.Item2[5]));
            armorLegs.Set(JsonHelper.FromJson<Armor>(database.Item2[6]));
            accessories.Set(JsonHelper.FromJson<Accessory>(database.Item2[7]));
            enemies.Set(JsonHelper.FromJson<Enemy>(database.Item2[8]));
            floorEventEnemies.Set(JsonHelper.FromJson<FloorEventEnemy>(database.Item2[9]));
            ailments.Set(JsonHelper.FromJson<Ailment>(database.Item2[10]));
            var enemyCharacterAttributes = new EnemyCharacterAttribute.Group();
            enemyCharacterAttributes.Set(JsonHelper.FromJson<EnemyCharacterAttribute>(database.Item2[11]));
            var wallEvents = new WallEvent.Group();
            wallEvents.Set(JsonHelper.FromJson<WallEvent>(database.Item2[12]));
            wallEventConditionItems.Set(JsonHelper.FromJson<WallEventConditionItem>(database.Item2[13]));
            dungeonSpecs.Set(JsonHelper.FromJson<DungeonSpec>(database.Item2[14]));
            itemTables.Set(JsonHelper.FromJson<ItemTable>(database.Item2[15]));
            var floorItemNoCosts = new FloorItemNoCost.Group();
            floorItemNoCosts.Set(JsonHelper.FromJson<FloorItemNoCost>(database.Item2[16]));
            var floorItemEnemyPlaces = new FloorItemEnemyPlace.Group();
            floorItemEnemyPlaces.Set(JsonHelper.FromJson<FloorItemEnemyPlace>(database.Item2[17]));
            enemyTables.Set(JsonHelper.FromJson<EnemyTable>(database.Item2[18]));
            foreach (var i in skills.List)
            {
                i.ActionSequences = AssetDatabase.LoadAssetAtPath<ScriptableSequences>($"Assets/SoulRPG/Database/SkillActions/{i.Id}.asset");
                if (i.ActionSequences == null)
                {
                    Debug.LogWarning($"Not found SkillAction {i.Id}");
                }
            }
            foreach (var i in enemies.List)
            {
                if (enemyCharacterAttributes.TryGetValue(i.Id, out var attributes))
                {
                    foreach (var a in attributes)
                    {
                        i.Attribute |= a.Attribute;
                    }
                }
                else
                {
                    i.Attribute = Define.CharacterAttribute.None;
                }
            }
            foreach (var i in ailments.List)
            {
                i.Sequences = AssetDatabase.LoadAssetAtPath<AilmentSequences>($"Assets/SoulRPG/Database/AilmentSequences/{i.Id}.asset");
                if (i.Sequences == null)
                {
                    Debug.LogWarning($"Not found AilmentSequences {i.Id}");
                }
            }
            foreach (var i in floorItemNoCosts.List)
            {
                var dungeonSpec = dungeonSpecs.Get(i.Key);
                Assert.IsNotNull(dungeonSpec, $"Not found DungeonSpec {i.Key}");
                dungeonSpec.FloorItemNoCosts = i.Value;
            }
            foreach (var i in floorItemEnemyPlaces.List)
            {
                var dungeonSpec = dungeonSpecs.Get(i.Key);
                Assert.IsNotNull(dungeonSpec, $"Not found DungeonSpec {i.Key}");
                dungeonSpec.FloorItemEnemyPlaces = i.Value;
            }
            foreach (var i in wallEvents.List)
            {
                var dungeonSpec = dungeonSpecs.Get(i.Key);
                Assert.IsNotNull(dungeonSpec, $"Not found DungeonSpec {i.Key}");
                dungeonSpec.WallEvents = i.Value;
            }
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            Debug.Log("End MasterData Update");
        }
#endif

        [Serializable]
        public class SpreadSheetDungeonCellData
        {
            public int x;

            public int y;

            public SpreadSheetBorder borders;

            [Serializable]
            public sealed class DictionaryList : DictionaryList<Vector2Int, SpreadSheetDungeonCellData>
            {
                public DictionaryList() : base(x => new Vector2Int(x.x, x.y)) { }
            }
        }

        [Serializable]
        public class SpreadSheetBorder
        {
            public bool top;

            public bool bottom;

            public bool left;

            public bool right;
        }

        [Serializable]
        public class DungeonWall : IEquatable<DungeonWall>
        {
            public Vector2Int a;

            public Vector2Int b;

            public bool Equals(DungeonWall other)
            {
                return a == other.a && b == other.b;
            }

            public override int GetHashCode()
            {
                return a.GetHashCode() ^ b.GetHashCode();
            }

            [Serializable]
            public sealed class DictionaryList : DictionaryList<(Vector2Int, Vector2Int), DungeonWall>
            {
                public DictionaryList() : base(x => (x.a, x.b)) { }
            }
        }

        [Serializable]
        public class Dungeon
        {
            public string name;

            public DungeonWall.DictionaryList wall = new();

            public Vector2Int range;

            [Serializable]
            public sealed class DictionaryList : DictionaryList<string, Dungeon>
            {
                public DictionaryList() : base(x => x.name) { }
            }

            public static Dungeon Create(string name, string wallData)
            {
                var cellData = new SpreadSheetDungeonCellData.DictionaryList();
                cellData.Set(JsonHelper.FromJson<SpreadSheetDungeonCellData>(wallData));
                var dw = new List<DungeonWall>();
                foreach (var i in cellData.List)
                {
                    if (i.borders.top)
                    {
                        dw.Add(new DungeonWall
                        {
                            a = new Vector2Int(i.x, i.y),
                            b = new Vector2Int(i.x + 1, i.y)
                        });
                    }
                    if (i.borders.bottom)
                    {
                        dw.Add(new DungeonWall
                        {
                            a = new Vector2Int(i.x, i.y - 1),
                            b = new Vector2Int(i.x + 1, i.y - 1)
                        });
                    }
                    if (i.borders.left)
                    {
                        dw.Add(new DungeonWall
                        {
                            a = new Vector2Int(i.x, i.y),
                            b = new Vector2Int(i.x, i.y - 1)
                        });
                    }
                    if (i.borders.right)
                    {
                        dw.Add(new DungeonWall
                        {
                            a = new Vector2Int(i.x + 1, i.y),
                            b = new Vector2Int(i.x + 1, i.y - 1)
                        });
                    }
                }
                var result = new Dungeon
                {
                    name = name
                };
                result.wall.Set(dw.Distinct());
                result.range = new Vector2Int(cellData.List.Max(x => x.x), cellData.List.Max(x => x.y));
                return result;
            }
        }

        [Serializable]
        public class FloorEventEnemy
        {
            public int Id;

            public string EventId;

            public int EnemyId;

            [Serializable]
            public class DictionaryList : DictionaryList<string, FloorEventEnemy>
            {
                public DictionaryList() : base(x => x.EventId) { }
            }
        }

        [Serializable]
        public class FloorItemNoCost
        {
            public int Id;

            public string DungeonName;

            public int X;

            public int Y;

            public int ItemTableId;

            [Serializable]
            public class Group : Group<string, FloorItemNoCost>
            {
                public Group() : base(x => x.DungeonName) { }
            }
        }

        [Serializable]
        public class FloorItemEnemyPlace
        {
            public int Id;

            public string DungeonName;

            public int X;

            public int Y;

            public int ItemTableId;

            [Serializable]
            public class Group : Group<string, FloorItemEnemyPlace>
            {
                public Group() : base(x => x.DungeonName) { }
            }
        }

        [Serializable]
        public class Item
        {
            public string Name;

            public int Id;

            [Serializable]
            public class DictionaryList : DictionaryList<int, Item>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class Weapon
        {
            public int ItemId;

            public int Strength;

            public Define.AttackAttribute AttackAttribute;

            public int Skill1;

            public int Skill2;

            public int Skill3;

            public int Skill4;

            public IEnumerable<int> Skills => new[] { Skill1, Skill2, Skill3, Skill4 };

            [Serializable]
            public class DictionaryList : DictionaryList<int, Weapon>
            {
                public DictionaryList() : base(x => x.ItemId) { }
            }
        }

        [Serializable]
        public class Skill
        {
            public string Name;

            public int Id;

            public int Cost;

            public bool CanRegisterUsedSkills;

            public string Description;

            public ScriptableSequences ActionSequences;

            [Serializable]
            public class DictionaryList : DictionaryList<int, Skill>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class Armor
        {
            public int ItemId;

            public float SlashCutRate;

            public float BlowCutRate;

            public float ThrustCutRate;

            public float MagicCutRate;

            public float FireCutRate;

            public float ThunderCutRate;

            [Serializable]
            public class DictionaryList : DictionaryList<int, Armor>
            {
                public DictionaryList() : base(x => x.ItemId) { }
            }
        }

        [Serializable]
        public class Accessory
        {
            public int ItemId;

            [Serializable]
            public class DictionaryList : DictionaryList<int, Accessory>
            {
                public DictionaryList() : base(x => x.ItemId) { }
            }
        }

        [Serializable]
        public class Enemy
        {
            public int Id;

            public string Name;

            public int HitPoint;

            public int Stamina;

            public int PhysicalAttack;

            public int MagicalAttack;

            public float SlashCutRate;

            public float BlowCutRate;

            public float ThrustCutRate;

            public float MagicCutRate;

            public float FireCutRate;

            public float ThunderCutRate;

            public int Speed;

            public int Experience;

            public int BehaviourPoint;

            public Define.CharacterAttribute Attribute;

            [Serializable]
            public class DictionaryList : DictionaryList<int, Enemy>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class EnemyCharacterAttribute
        {
            public int EnemyId;

            public Define.CharacterAttribute Attribute;

            [Serializable]
            public class Group : Group<int, EnemyCharacterAttribute>
            {
                public Group() : base(x => x.EnemyId) { }
            }
        }

        [Serializable]
        public class Ailment
        {
            public int Id;

            public string Name;

            public string Description;

            public bool CanReset;

            public AilmentSequences Sequences;

            [Serializable]
            public class DictionaryList : DictionaryList<int, Ailment>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class WallEvent
        {
            public string Id;

            public string DungeonName;

            public int LeftX;

            public int LeftY;

            public int RightX;

            public int RightY;

            public string EventType;

            public string PositiveSideCondition;

            public string NegativeSideCondition;

            public float CreateRate;

            [Serializable]
            public class DictionaryList : DictionaryList<string, (string, int, int, int, int), WallEvent>
            {
                public DictionaryList() : base(
                    x => x.Id,
                    x => (x.DungeonName, x.LeftX, x.LeftY, x.RightX, x.RightY)
                    )
                { }
            }

            [Serializable]
            public class Group : Group<string, WallEvent>
            {
                public Group() : base(x => x.DungeonName) { }
            }

            public WallPosition GetWallPosition()
            {
                return new WallPosition(LeftX, LeftY, RightX, RightY);
            }
        }

        [Serializable]
        public class WallEventConditionItem : INeedItem
        {
            public int Id;

            public string EventId;

            public int ItemId;

            public int Count;

            int INeedItem.ItemId => ItemId;

            int INeedItem.Count => Count;

            [Serializable]
            public class Group : Group<string, WallEventConditionItem>
            {
                public Group() : base(x => x.EventId) { }
            }
        }

        [Serializable]
        public class DungeonSpec
        {
            public string Id;

            public int InitialX;

            public int InitialY;

            public int NoCostItemNumberMin;

            public int NoCostItemNumberMax;

            public int EnemyPlaceItemNumberMin;

            public int EnemyPlaceItemNumberMax;

            public List<FloorItemNoCost> FloorItemNoCosts;

            public List<FloorItemEnemyPlace> FloorItemEnemyPlaces;

            public List<WallEvent> WallEvents;

            [Serializable]
            public class DictionaryList : DictionaryList<string, DungeonSpec>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class ItemTable : IWeight
        {
            public int Id;

            public int TableId;

            public int ItemId;

            public int Count;

            public int Weight;

            int IWeight.Weight => Weight;

            [Serializable]
            public class Group : Group<int, ItemTable>
            {
                public Group() : base(x => x.TableId) { }
            }
        }

        [Serializable]
        public class EnemyTable
        {
            public int Id;

            public int TableId;

            public int EnemyId;

            public int Weight;

            [Serializable]
            public class Group : Group<int, EnemyTable>
            {
                public Group() : base(x => x.TableId) { }
            }
        }
    }
}
