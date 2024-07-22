using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using HK;
using UnityEngine;

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
        private DungeonEvent.DictionaryList dungeonEvents;
        public DungeonEvent.DictionaryList DungeonEvents => dungeonEvents;

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
            };
            var dungeonDownloader = UniTask.WhenAll(
                dungeonNames.Select(GoogleSpreadSheetDownloader.DownloadAsync)
            );
            var masterDataDownloader = UniTask.WhenAll(
                masterDataNames.Select(GoogleSpreadSheetDownloader.DownloadAsync)
            );
            var database = await UniTask.WhenAll(dungeonDownloader, masterDataDownloader);
            dungeons.Set(database.Item1.Select((x, i) => Dungeon.Create(dungeonNames[i], x)));
            dungeonEvents.Set(JsonHelper.FromJson<DungeonEvent>(database.Item2[0]));
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
        public class DungeonEvent
        {
            public int Id;

            public string DungeonName;

            public int X;

            public int Y;

            public string EventType;
            
            [Serializable]
            public class DictionaryList : DictionaryList<int, DungeonEvent>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }
    }
}
