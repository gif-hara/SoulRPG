using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using HK;
using UnityEditor;
using UnityEngine;
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
        private FloorEvent.DictionaryList floorEvents;
        public FloorEvent.DictionaryList FloorEvents => floorEvents;

        [SerializeField]
        private FloorEventItem.Group floorEventItems;
        public FloorEventItem.Group FloorEventItems => floorEventItems;

        [SerializeField]
        private FloorEventEnemy.DictionaryList floorEventEnemies;
        public FloorEventEnemy.DictionaryList FloorEventEnemies => floorEventEnemies;

        [SerializeField]
        private WallEvent.DictionaryList wallEvents;
        public WallEvent.DictionaryList WallEvents => wallEvents;

        [SerializeField]
        private Item.DictionaryList items;
        public Item.DictionaryList Items => items;

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
                "MasterData.DungeonEvent",
                "MasterData.DungeonEvent.Item",
                "MasterData.Item",
                "MasterData.Weapon",
                "MasterData.Skill",
                "MasterData.Armor.Head",
                "MasterData.Armor.Body",
                "MasterData.Armor.Arms",
                "MasterData.Armor.Legs",
                "MasterData.Accessory",
                "MasterData.Enemy",
                "MasterData.DungeonEvent.Enemy",
                "MasterData.Ailment",
                "MasterData.Enemy.CharacterAttribute",
                "MasterData.WallEvent",
            };
            var dungeonDownloader = UniTask.WhenAll(
                dungeonNames.Select(GoogleSpreadSheetDownloader.DownloadAsync)
            );
            var masterDataDownloader = UniTask.WhenAll(
                masterDataNames.Select(GoogleSpreadSheetDownloader.DownloadAsync)
            );
            var database = await UniTask.WhenAll(dungeonDownloader, masterDataDownloader);
            dungeons.Set(database.Item1.Select((x, i) => Dungeon.Create(dungeonNames[i], x)));
            floorEvents.Set(JsonHelper.FromJson<FloorEvent>(database.Item2[0]));
            floorEventItems.Set(JsonHelper.FromJson<FloorEventItem>(database.Item2[1]));
            items.Set(JsonHelper.FromJson<Item>(database.Item2[2]));
            weapons.Set(JsonHelper.FromJson<Weapon>(database.Item2[3]));
            skills.Set(JsonHelper.FromJson<Skill>(database.Item2[4]));
            foreach (var i in skills.List)
            {
                i.ActionSequences = AssetDatabase.LoadAssetAtPath<ScriptableSequences>($"Assets/SoulRPG/Database/SkillActions/{i.Id}.asset");
                if (i.ActionSequences == null)
                {
                    Debug.LogWarning($"Not found SkillAction {i.Id}");
                }
            }
            armorHeads.Set(JsonHelper.FromJson<Armor>(database.Item2[5]));
            armorBodies.Set(JsonHelper.FromJson<Armor>(database.Item2[6]));
            armorArms.Set(JsonHelper.FromJson<Armor>(database.Item2[7]));
            armorLegs.Set(JsonHelper.FromJson<Armor>(database.Item2[8]));
            accessories.Set(JsonHelper.FromJson<Accessory>(database.Item2[9]));
            enemies.Set(JsonHelper.FromJson<Enemy>(database.Item2[10]));
            floorEventEnemies.Set(JsonHelper.FromJson<FloorEventEnemy>(database.Item2[11]));
            ailments.Set(JsonHelper.FromJson<Ailment>(database.Item2[12]));
            var enemyCharacterAttributes = new EnemyCharacterAttribute.Group();
            enemyCharacterAttributes.Set(JsonHelper.FromJson<EnemyCharacterAttribute>(database.Item2[13]));
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
            wallEvents.Set(JsonHelper.FromJson<WallEvent>(database.Item2[14]));
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
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
        public class FloorEvent
        {
            public string Id;

            public string DungeonName;

            public int X;

            public int Y;

            public string EventType;

            public bool IsOneTime;

            [Serializable]
            public class DictionaryList : DictionaryList<string, (string, int, int), FloorEvent>
            {
                public DictionaryList() : base(
                    x => x.Id,
                    x => (x.DungeonName, x.X, x.Y)
                )
                {
                }
            }
        }

        [Serializable]
        public class FloorEventItem
        {
            public int Id;

            public string EventId;

            public int ItemId;

            public int Count;

            [Serializable]
            public class Group : Group<string, FloorEventItem>
            {
                public Group() : base(x => x.EventId) { }
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

            public int Speed;

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

            public int BehaviourPriority;

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

            [Serializable]
            public class DictionaryList : DictionaryList<string, (string, int, int, int, int), WallEvent>
            {
                public DictionaryList() : base(
                    x => x.Id,
                    x => (x.DungeonName, x.LeftX, x.LeftY, x.RightX, x.RightY)
                    )
                { }
            }
        }
    }
}
