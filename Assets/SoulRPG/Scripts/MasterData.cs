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

#if UNITY_EDITOR
        [ContextMenu("Update")]
        private async void UpdateMasterData()
        {
            Debug.Log("Begin MasterData Update");
            var dungeonNames = new string[]
            {
                "Dungeon.Test"
            };
            var database = await UniTask.WhenAll(
                dungeonNames.Select(x => GoogleSpreadSheetDownloader.DownloadAsync(x))
            );
            dungeons.Set(database.Select((x, i) => Dungeon.Create(dungeonNames[i], x)));

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
            public Vector2Int position;

            public bool Equals(DungeonWall other)
            {
                return position == other.position;
            }

            public override int GetHashCode()
            {
                return position.GetHashCode();
            }

            [Serializable]
            public sealed class DictionaryList : DictionaryList<Vector2Int, DungeonWall>
            {
                public DictionaryList() : base(x => x.position) { }
            }
        }

        [Serializable]
        public class Dungeon
        {
            public string name;

            public DungeonWall.DictionaryList wall = new();

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
                        dw.Add(new DungeonWall { position = new Vector2Int(i.x, i.y) });
                        dw.Add(new DungeonWall { position = new Vector2Int(i.x + 1, i.y) });
                    }
                    if (i.borders.bottom)
                    {
                        dw.Add(new DungeonWall { position = new Vector2Int(i.x, i.y + 1) });
                        dw.Add(new DungeonWall { position = new Vector2Int(i.x + 1, i.y + 1) });
                    }
                    if (i.borders.left)
                    {
                        dw.Add(new DungeonWall { position = new Vector2Int(i.x, i.y) });
                        dw.Add(new DungeonWall { position = new Vector2Int(i.x, i.y + 1) });
                    }
                    if (i.borders.right)
                    {
                        dw.Add(new DungeonWall { position = new Vector2Int(i.x + 1, i.y) });
                        dw.Add(new DungeonWall { position = new Vector2Int(i.x + 1, i.y + 1) });
                    }
                }
                var result = new Dungeon
                {
                    name = name
                };
                result.wall.Set(dw.Distinct());
                return result;
            }
        }
    }
}
